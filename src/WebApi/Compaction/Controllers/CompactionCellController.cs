using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using WGSPoint = VSS.Productivity3D.Models.Models.WGSPoint3D;


namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting production data cell value from Raptor
  /// </summary>
  [ProjectUidVerifier]
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionCellController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Constructor with a dependency injection.
    /// </summary>
    /// <param name="raptorClient">>Raptor client.</param>
    /// <param name="loggerFactory">Logger.</param>
    /// <param name="configStore">Configuration store.</param>
    /// <param name="fileListProxy">File list proxy.</param>
    /// <param name="projectSettingsProxy">Project settings proxy.</param>
    /// <param name="settingsManager">Compaction settings manager.</param>
    /// <param name="serviceExceptionHandler">Service exception handler.</param>
    /// <param name="filterServiceProxy">Filter service proxy.</param>
    /// <param name="requestFactory">The request factory.</param>
    public CompactionCellController(IASNodeClient raptorClient, ILoggerFactory loggerFactory, IConfigurationStore configStore, 
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IServiceExceptionHandler serviceExceptionHandler, IFilterServiceProxy filterServiceProxy, IProductionDataRequestFactory requestFactory) 
      : base(loggerFactory, loggerFactory.CreateLogger<CompactionCellController>(), serviceExceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
    }

    // GET: api/Cells
    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc.
    /// The cell is identified by either WGS84 lat/long coordinates.
    /// </summary>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    /// <executor>CellDatumExecutor</executor> 
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

      CellDatumRequest request = CellDatumRequest.CreateCellDatumRequest(
        projectId, 
        displayMode,
        WGSPoint.CreatePoint(lat.LatDegreesToRadians(), lon.LonDegreesToRadians()), 
        null, 
        filter, 
        filter?.Id ?? -1, 
        liftSettings, 
        cutFillDesign);

      request.Validate();

      return RequestExecutorContainerFactory.Build<CompactionCellDatumExecutor>(LoggerFactory, raptorClient).Process(request) as CompactionCellDatumResult;
    }
  }
}
