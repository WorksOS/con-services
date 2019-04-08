using System;
using System.IO;
using System.Linq;
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
using VSS.TRex.Gateway.Common.Helpers;
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
      if (patchRequest.ProjectUid == null || patchRequest.ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      var siteModel = GatewayHelper.ValidateAndGetSiteModel(patchRequest.ProjectUid.Value, nameof(PostSubGridPatches));
      if (patchRequest.Filter1 != null && patchRequest.Filter1.ContributingMachines != null)
        GatewayHelper.ValidateMachines(patchRequest.Filter1.ContributingMachines.Select(m => m.AssetUid).ToList(), siteModel);
      if (patchRequest.Filter2 != null && patchRequest.Filter2.ContributingMachines != null)
        GatewayHelper.ValidateMachines(patchRequest.Filter2.ContributingMachines.Select(m => m.AssetUid).ToList(), siteModel);


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
      var siteModel = GatewayHelper.ValidateAndGetSiteModel(cellDatumRequest.ProjectUid, nameof(CompactionCellDatumResult));
      if (cellDatumRequest.Filter != null && cellDatumRequest.Filter.ContributingMachines != null)
        GatewayHelper.ValidateMachines(cellDatumRequest.Filter.ContributingMachines.Select(m => m.AssetUid).ToList(), siteModel);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CellDatumExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cellDatumRequest) as CompactionCellDatumResult);
    }
  }
}
