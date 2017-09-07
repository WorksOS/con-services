using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionExportController : BaseController
  {
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private readonly IPreferenceProxy prefProxy;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configStore">Configuration store</param>/// 
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public CompactionExportController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, IPreferenceProxy prefProxy) :
      base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      log = logger.CreateLogger<CompactionExportController>();
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.prefProxy = prefProxy;
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Gets an export of 3D project data in .TTM file format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="tolerance">Controls triangulation density in the output .TTM file.</param>
    /// <param name="filterUid">The filter Uid to apply to the export results</param>
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/surface")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportSurface(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid)
    {
      const double SURFACE_EXPORT_TOLLERANCE = 0.05;

      log.LogInformation("GetExportReportSurface: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid, null, null, null, null, null, null, null, null, null);

      tolerance = tolerance ?? SURFACE_EXPORT_TOLLERANCE;

      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetPreferencesProxy(prefProxy)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
        .CreateExportRequest(
          projectUid,
          null, //startUtc,
          null, //endUtc,
          CoordTypes.ptNORTHEAST,
          ExportTypes.kSurfaceExport,
          fileName,
          false,
          false,
          OutputTypes.etVedaAllPasses,
          "",
          tolerance.Value);

      exportRequest.Validate();

      return RequestExecutorContainerFactory
        .Build<ExportReportExecutor>(logger, raptorClient, null, configStore)
        .Process(exportRequest) as ExportResult;
    }

    /// <summary>
    /// Gets an export of production data in cell grid format report for import to VETA.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="machineNames">Comma-separated list of machine names.</param>
    /// <param name="filterUid">The filter Uid to apply to the export results</param>
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/veta")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportVeta(
      [FromQuery] Guid projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetExportReportVeta: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid, null, null, null, null, null, null, null, null, null);

      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetRaptorClient(raptorClient)
        .SetPreferencesProxy(prefProxy)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
        .CreateExportRequest(
          projectUid,
          startUtc,
          endUtc,
          CoordTypes.ptNORTHEAST,
          ExportTypes.kVedaExport,
          fileName,
          false,
          true,
          OutputTypes.etVedaAllPasses,
          machineNames);

      exportRequest.Validate();

      return RequestExecutorContainerFactory
        .Build<ExportReportExecutor>(logger, raptorClient, null, configStore)
        .Process(exportRequest) as ExportResult;
    }

    /// <summary>
    /// Gets an export of production data in cell grid format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC.</param>
    /// <param name="coordType">Either Northing/Easting or Latitude/Longitude.</param>
    /// <param name="outputType">Either all passes/last for pass machine passes export or all passes/final pass for export for VETA</param>
    /// <param name="restrictOutput">Output .CSV file is restricted to 65535 rows if it is true.</param>
    /// <param name="rawDataOutput">Column headers in an output .CSV file's are in the dBase format.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="filterUid">The filter Uid to apply to the export results</param>
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/machinepasses")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportMachinePasses(
      [FromQuery] Guid projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] int coordType,
      [FromQuery] int outputType,
      [FromQuery] bool restrictOutput,
      [FromQuery] bool rawDataOutput,
      [FromQuery] string fileName,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetExportReportMachinePasses: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid, null, null, null, null, null, null, null, null, null);

      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetPreferencesProxy(prefProxy)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
        .CreateExportRequest(
          projectUid,
          startUtc,
          endUtc,
          (CoordTypes)coordType,
          ExportTypes.kPassCountExport,
          fileName,
          restrictOutput,
          rawDataOutput,
          (OutputTypes)outputType,
          "");

      exportRequest.Validate();

      return RequestExecutorContainerFactory
        .Build<ExportReportExecutor>(logger, raptorClient, null, configStore)
        .Process(exportRequest) as ExportResult;
    }
  }
}