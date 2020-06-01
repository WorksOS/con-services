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
using VSS.Productivity3D.TagFileGateway.Common.Executors;
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
      [FromServices] ITransferProxy transferProxy,
      [FromServices] IWebRequest webRequest)
    {
      var isDirect = Request.Path.Value.Contains("/direct");
      _logger.LogInformation($"Attempting to process {(isDirect ? "Direct" : "Non-Direct")} tag file {request?.FileName}");
      var result = await RequestExecutorContainer
        .Build<TagFileProcessExecutor>(loggerFactory, configStore, dataCache, tagFileForwarder, transferProxy, webRequest)
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

      var result = await RequestExecutorContainer.Build<TagFileSnsProcessExecutor>(loggerFactory,
          configStore,
          dataCache,
          tagFileForwarder,
          transferProxy,
          webRequest)
        .ProcessAsync(payload);

        if(result != null)
          return Ok();
        // Note sure if we return bad request or not on failed processing - will updated if needed
        return BadRequest();
    }
  }
}
