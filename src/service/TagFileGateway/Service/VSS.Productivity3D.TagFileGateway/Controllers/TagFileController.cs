using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;
using VSS.Productivity3D.TagFileGateway.Common.Models.Executors;

namespace VSS.Productivity3D.TagFileGateway.Controllers
{
  public class TagFileController : Controller
  {

    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostTagFileNonDirectSubmission([FromBody] CompactionTagFileRequest request, 
      [FromServices] ILogger logger, 
      [FromServices] IConfigurationStore configStore, 
      [FromServices] IDataCache dataCache, 
      [FromServices] ITagFileForwarder tagFileForwarder,
      [FromServices] ITransferProxy transferProxy)
    {
      logger.LogInformation($"Attempting to process non-Direct tag file {request?.FileName}");
      var result = await RequestExecutorContainer
        .Build<TagFileProcessExecutor>(logger, configStore, dataCache, tagFileForwarder, transferProxy)
        .ProcessAsync(request);

      logger.LogInformation($"Got result {JsonConvert.SerializeObject(result)} for Tag file: {request?.FileName}");
      return result;
    }

    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostTagFileDirectSubmission([FromBody] CompactionTagFileRequest request,
      [FromServices] ILogger logger, 
      [FromServices] IConfigurationStore configStore, 
      [FromServices] IDataCache dataCache, 
      [FromServices] ITagFileForwarder tagFileForwarder,
      [FromServices] ITransferProxy transferProxy)
    {
      logger.LogInformation($"Attempting to process Direct tag file {request?.FileName}");
      var result = await RequestExecutorContainer
        .Build<TagFileProcessExecutor>(logger, configStore, dataCache, tagFileForwarder, transferProxy)
        .ProcessAsync(request);

      logger.LogInformation($"Got result {JsonConvert.SerializeObject(result)} for Tag file: {request?.FileName}");
      return result;
    }
  }
}
