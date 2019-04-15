using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
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
    public FileResult PostSubGridPatches([FromBody] PatchDataRequest patchRequest)
    {
      Log.LogInformation($"{nameof(PostSubGridPatches)}: {Request.QueryString}");

      patchRequest.Validate();
      ValidateFilterMachines(nameof(PostSubGridPatches), patchRequest.ProjectUid, patchRequest.Filter1);
      ValidateFilterMachines(nameof(PostSubGridPatches), patchRequest.ProjectUid, patchRequest.Filter2);
      
      var patchResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<PatchRequestExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(patchRequest)) as PatchDataResult;

      if (patchResult?.PatchData == null)
      {
        var code = patchResult == null ? HttpStatusCode.BadRequest : HttpStatusCode.NoContent;
        var exCode = patchResult == null ? ContractExecutionStatesEnum.FailedToGetResults : ContractExecutionStatesEnum.ValidationError;

        throw new ServiceException(code, new ContractExecutionResult(exCode, $"Failed to get subgrid patches for project ID: {patchRequest.ProjectUid}"));
      }

      return new FileStreamResult(new MemoryStream(patchResult?.PatchData), "application/octet-stream");
    }

    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc.
    /// The cell is identified by either WGS84 lat/long coordinates.
    /// </summary>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    [HttpPost("cells/datum")]
    public CompactionCellDatumResult PostCellDatum([FromBody] CellDatumTRexRequest cellDatumRequest)
    {
      Log.LogInformation($"{nameof(PostCellDatum)}: {Request.QueryString}");

      cellDatumRequest.Validate();
      ValidateFilterMachines(nameof(PostCellDatum), cellDatumRequest.ProjectUid, cellDatumRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CellDatumExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cellDatumRequest) as CompactionCellDatumResult);
    }
  }
}
