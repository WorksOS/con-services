using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors.CellPass;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting production data cell values from TRex
  /// </summary>
  [ProjectVerifier]
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionCellController : BaseController<CompactionCellController>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionCellController(IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
      : base(configStore, fileImportProxy, settingsManager)
    { }

    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc.
    /// The cell is identified by either WGS84 lat/long coordinates.
    /// </summary>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    [HttpGet("api/v2/productiondata/cells/datum")]
    public async Task<CompactionCellDatumResult> GetProductionDataCellsDatum(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] DisplayMode displayMode,
      [FromQuery] double lat,
      [FromQuery] double lon)
    {
      Log.LogInformation("GetProductionDataCellsDatum: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid, filterMustExist: true);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var cutFillDesign = GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);

      await Task.WhenAll(projectId, filter, projectSettings, cutFillDesign);

      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings.Result);

      var request = new CellDatumRequest(
        projectId.Result,
        projectUid,
        displayMode,
        new WGSPoint(lat.LatDegreesToRadians(), lon.LonDegreesToRadians()),
        null,
        filter.Result,
        liftSettings,
        cutFillDesign.Result);

      request.Validate();

      return await RequestExecutorContainerFactory.Build<CompactionCellDatumExecutor>(LoggerFactory,
#if RAPTOR
        RaptorClient,
#endif
        configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as CompactionCellDatumResult;
    }

    /// <summary>
    /// Retrieve passes for a single cell and process them according to the provided filter and layer analysis parameters
    /// The version 2 endpoint supports FilterUID values being passed, as opposed to raw filters or filter ID values
    /// Other than that, the request and response is the same.
    /// </summary>
    [HttpGet("api/v2/productiondata/cells/passes")]
    public async Task<List<CellPassesV2Result.FilteredPassData>> CellPassesV2(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] double lat,
      [FromQuery] double lon)
    {
      Log.LogInformation("GetProductionDataCellsDatum: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid, filterMustExist: true);

      await Task.WhenAll(projectId, filter);

      var request = new CellPassesRequest
      {
        ProjectId = await projectId,
        ProjectUid = projectUid,
        filter = await filter,
        liftBuildSettings = new LiftBuildSettings
        {
          LiftDetectionType = LiftDetectionType.None
        },
        probePositionLL = new WGSPoint(lat.LatDegreesToRadians(), lon.LonDegreesToRadians())
      };

      request.Validate();
      var result = await RequestExecutorContainerFactory.Build<CellPassesV2Executor>(LoggerFactory,
#if RAPTOR
          RaptorClient,
#endif
          configStore: ConfigStore,
          trexCompactionDataProxy: TRexCompactionDataProxy,
          customHeaders: CustomHeaders)
        .ProcessAsync(request) as CellPassesV2Result;

      if (result?.Layers == null || result?.Layers.Length == 0)
        throw new ServiceException(HttpStatusCode.NoContent, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "No layers found"));

      if (result?.Layers.Length > 1)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Multiple layers found"));

      if (result?.Layers[0].PassData == null)
        throw new ServiceException(HttpStatusCode.NoContent, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "No cell passes found"));

      // With our lift settings set to None, we should have exactly 1 layer
      return result.Layers[0].PassData.ToList();
    }
  }
}
