using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;
using VSS.Productivity3D.TagFileGateway.Common.Models.Executors;
using VSS.Productivity3D.TagFileGateway.Common.Models.Sns;

namespace VSS.Productivity3D.TagFileGateway.Controllers
{
  public class TagFileController : Controller
  {
    private readonly ILogger<TagFileController> _logger;

    public TagFileController(ILogger<TagFileController> logger)
    {
      _logger = logger;
    }

    [Route("api/v2/tagfiles/direct")]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostTagFileNonDirectSubmission([FromBody] CompactionTagFileRequest request, 
      [FromServices] ILoggerFactory loggerFactory, 
      [FromServices] IConfigurationStore configStore, 
      [FromServices] IDataCache dataCache, 
      [FromServices] ITagFileForwarder tagFileForwarder,
      [FromServices] ITransferProxy transferProxy)
    {
      var isDirect = Request.Path.Value.Contains("/direct");
      _logger.LogInformation($"Attempting to process {(isDirect ? "Direct" : "Non-Direct")} tag file {request?.FileName}");
      var result = await RequestExecutorContainer
        .Build<TagFileProcessExecutor>(loggerFactory, configStore, dataCache, tagFileForwarder, transferProxy)
        .ProcessAsync(request);

      _logger.LogInformation($"Got result {JsonConvert.SerializeObject(result)} for Tag file: {request?.FileName}");

      
      // If we uploaded, return a successful result
      // (as the tag file may not have been processed for legitimate reasons)
      // We don't want the machine sending tag files over and over again in this instance
      return new ContractExecutionResult();
    }

    [Route("api/v2/tagfiles/sns")]
    public async Task<IActionResult> PostSnsTagFile(  
      [FromServices] IWebRequest webRequest,
      [FromServices] ILoggerFactory loggerFactory, 
      [FromServices] IConfigurationStore configStore, 
      [FromServices] IDataCache dataCache, 
      [FromServices] ITagFileForwarder tagFileForwarder,
      [FromServices] ITransferProxy transferProxy)
    {
      // https://forums.aws.amazon.com/thread.jspa?threadID=69413
      // AWS SNS is in text/plain, not application/json - so need to parse manually
      var payloadMs = new MemoryStream();
      await Request.Body.CopyToAsync(payloadMs);
      var payload = JsonConvert.DeserializeObject<SnsPayload>(Encoding.UTF8.GetString(payloadMs.ToArray()));

      if (payload == null)
        return BadRequest();

      _logger.LogInformation($"Sns message type: {payload.Type}, topic: {payload.TopicArn}");
      if (payload.Type == SnsPayload.SubscriptionType)
      {
        // Request for subscription
        _logger.LogInformation($"SNS SUBSCRIPTION REQUEST: {payload.Message}, Subscription URL: '{payload.SubscribeURL}'");
        return Ok();
      }

      if (payload.IsNotification)
      {
        // Got a tag file
        var tagFile = JsonConvert.DeserializeObject<SnsTagFile>(payload.Message);
        if (tagFile == null)
        {
          _logger.LogWarning($"Could not convert to Tag File Model. JSON: {payload.Message}");
          return BadRequest();
        }

        byte[] data;
        if(!string.IsNullOrEmpty(tagFile.DownloadUrl))
        {
          _logger.LogInformation($"Tag file {tagFile.FileName} needs to be downloaded from : {tagFile.DownloadUrl}");
          var downloadTagFileData = await webRequest.ExecuteRequestAsStreamContent(tagFile.DownloadUrl, HttpMethod.Get);
          await using var ms = new MemoryStream();
          await downloadTagFileData.CopyToAsync(ms);
          data = ms.ToArray();
          if (data.Length != tagFile.FileSize)
          {
            _logger.LogWarning($"Downloaded data length {data.Length} is not equal to expected length {tagFile.FileSize}");
          }
          _logger.LogInformation($"Downloaded tag file {tagFile.FileName}, total bytes: {data.Length}");
        }
        else
        {
          _logger.LogInformation($"Tag file data is included in payload for file {tagFile.FileName}");
          data = tagFile.Data;
        }

        var request = new CompactionTagFileRequest
        {
          Data = data, FileName = tagFile.FileName, OrgId = tagFile.OrgId
        };

        _logger.LogInformation($"Attempting to process sns tag file {tagFile?.FileName}");
        var result = await RequestExecutorContainer
          .Build<TagFileProcessExecutor>(loggerFactory, configStore, dataCache, tagFileForwarder, transferProxy)
          .ProcessAsync(request);

        _logger.LogInformation($"Got result {JsonConvert.SerializeObject(result)} for Tag file: {tagFile?.FileName}");

        if(result != null)
          return Ok();
        // Note sure if we return bad request or not on failed processing - will updated if needed
        return BadRequest();
      }

      _logger.LogWarning($"Unknown SNS Type: {payload.Type} - not sure how to process");
      return BadRequest();
    }
  }
}
