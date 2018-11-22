using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using WGSPoint = VSS.Productivity3D.Models.Models.WGSPoint3D;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting production data cell value from Raptor
  /// </summary>
  [ProjectVerifier]
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionCellController : BaseController<CompactionCellController>
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    protected readonly ITRexCompactionDataProxy TRexCompactionDataProxy;


    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionCellController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, ITRexCompactionDataProxy trexCompactionDataProxy)
      : base(configStore, fileListProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      TRexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc.
    /// The cell is identified by either WGS84 lat/long coordinates.
    /// </summary>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    [Route("api/v2/productiondata/cells/datum")]
    [HttpGet]
    public async Task<CompactionCellDatumResult> GetProductionDataCellsDatum(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] DisplayMode displayMode,
      [FromQuery] double lat,
      [FromQuery] double lon)
    {
      Log.LogInformation("GetProductionDataCellsDatum: " + Request.QueryString);

      var projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);
      var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);

      var request = CellDatumRequest.CreateCellDatumRequest(
        projectId,
        displayMode,
        new WGSPoint(lat.LatDegreesToRadians(), lon.LonDegreesToRadians()),
        null,
        filter,
        filter?.Id ?? -1,
        liftSettings,
        cutFillDesign);

      request.Validate();

      return RequestExecutorContainerFactory.Build<CompactionCellDatumExecutor>(LoggerFactory, raptorClient).Process(request) as CompactionCellDatumResult;
    }

    /// <summary>
    /// Gets the subgrid patches for a given project. Maybe be filtered with a polygon grid.
    /// </summary>
    /// <remarks>
    /// This endpoint is expected to be used by machine based devices requesting raw data and deliberately
    /// returns a lean response object to minimise the response size.
    /// The response DTOs are decorated for use with Protobuf-net.
    /// </remarks>
    /// <param name="projectUid">Project identifier</param>
    /// <param name="filterUid">Filter identifier (optional)</param>
    /// <param name="patchId">Id of the requested patch</param>
    /// <param name="mode">Desired data (0 for elevation)</param>
    /// <param name="patchSize">Number of cell subgrids horizontally/vertically in a square patch (each subgrid has 32 cells)</param>
    /// <param name="includeTimeOffsets">If set, includes the time when the cell was recorded as a value expressed as Unix UTC time.</param>
    /// <returns>Returns a highly efficient response stream of patch information (using Protobuf protocol).</returns>
    [ProjectVerifier]
    [Route("api/v2/patches")]
    [HttpGet]
    public async Task<IActionResult> GetSubGridPatches(Guid projectUid, Guid filterUid, int patchId, DisplayMode mode, int patchSize, bool includeTimeOffsets = false)
    {
      Log.LogInformation($"GetSubGridPatches: {Request.QueryString}");

      var projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(await GetProjectSettingsTargets(projectUid));

      var patchRequest = new PatchRequest(
        projectId,
        projectUid,
        new Guid(),
        mode,
        null,
        liftSettings,
        false,
        VolumesType.None,
        VelociraptorConstants.VOLUME_CHANGE_TOLERANCE,
        null, filter, filter?.Id ?? 0, null, 0, FilterLayerMethod.AutoMapReset, patchId, patchSize, includeTimeOffsets);

      patchRequest.Validate();

      var v2PatchRequestResponse = WithServiceExceptionTryExecute(() => RequestExecutorContainerFactory
        .Build<CompactionPatchV2Executor>(LoggerFactory, raptorClient, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
        .Process(patchRequest) as CompactionExportResult);

      return Ok(v2PatchRequestResponse);
    }
  }
}
