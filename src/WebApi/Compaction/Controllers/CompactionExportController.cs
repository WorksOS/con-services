using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApiModels.Report.Executors;

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
    /// <param name="prefProxy">User preferences proxy</param>
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
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();


      tolerance = tolerance ?? SURFACE_EXPORT_TOLLERANCE;

      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
        .CreateExportRequest(
          null, //startUtc,
          null, //endUtc,
          CoordTypes.ptNORTHEAST,
          ExportTypes.kSurfaceExport,
          fileName,
          false,
          false,
          OutputTypes.etVedaAllPasses,
          string.Empty,
          tolerance.Value);

      exportRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<ExportReportExecutor>(logger, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as ExportResult
      );
    }

    /// <summary>
    /// Gets an export of production data in cell grid format report for import to VETA.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="machineNames">Comma-separated list of machine names.</param>
    /// <param name="filterUid">The filter Uid to apply to the export results</param>
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/veta")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportVeta(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetExportReportVeta: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();

      DateTime startUtc, endUtc;
      GetDateRange(projectId, filter, out startUtc, out endUtc);
    
      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetRaptorClient(raptorClient)
        .SetUserPreferences(userPreferences)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
        .CreateExportRequest(
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

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<ExportReportExecutor>(logger, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as ExportResult
      );
    }

    /// <summary>
    /// Gets an export of production data in cell grid format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
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
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();

      DateTime startUtc, endUtc;
      GetDateRange(projectId, filter, out startUtc, out endUtc);

      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
        .CreateExportRequest(
          startUtc,
          endUtc,
          (CoordTypes)coordType,
          ExportTypes.kPassCountExport,
          fileName,
          restrictOutput,
          rawDataOutput,
          (OutputTypes)outputType,
          string.Empty);

      exportRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<ExportReportExecutor>(logger, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as ExportResult
      );
    }

    /// <summary>
    /// Get user preferences
    /// </summary>
    /// <returns></returns>
    private async Task<UserPreferenceData> GetUserPreferences()
    {
      var userPreferences = await prefProxy.GetUserPreferences(this.CustomHeaders);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }
      return userPreferences;
    }

    /// <summary>
    /// Gets the date range for the export.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="filter"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    private void GetDateRange(long projectId, Common.Models.Filter filter, out DateTime startUtc, out DateTime endUtc)
    {
      if (filter == null || !filter.StartUtc.HasValue || !filter.EndUtc.HasValue)
      {
        //Special case of project extents where start and end UTC not set in filter for Raptor peformance.
        //But need to set here for export.
        var excludedIds = filter?.SurveyedSurfaceExclusionList?.ToArray() ?? new long[0];
        ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, excludedIds);
        request.Validate();

        var result =
          RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient)
            .Process(request) as ProjectStatisticsResult;

        startUtc = result.startTime;
        endUtc = result.endTime;
      }
      else
      {
        startUtc = filter.StartUtc.Value;
        endUtc = filter.EndUtc.Value;

      }
    }
  }
}