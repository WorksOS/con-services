using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data cell value and subgrid patches .
  /// </summary>
  [Route("api/v1")]
  public class CellController : BaseController
  {
    public CellController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore) : 
      base(loggerFactory, loggerFactory.CreateLogger<CellController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Gets the subgrid patches for a given project. Maybe be filtered with a polygon grid.
    /// </summary>
    /// <param name="patchRequest"></param>
    /// <returns>Returns a highly efficient response stream of patch information (using Protobuf protocol).</returns>
    [HttpPost("patches")]
    public FileResult GetSubGridPatches([FromBody] PatchDataRequest patchRequest)
    {
      Log.LogInformation($"{nameof(GetSubGridPatches)}: {Request.QueryString}");

      patchRequest.Validate();

      var patchResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<PatchRequestExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(patchRequest)) as PatchDataResult;

      return new FileStreamResult(patchResult?.PatchData, "application/octet-stream");
    }
  }
}
