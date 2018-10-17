using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  //NOTE: do not cache responses as large amount of data
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  [ProjectVerifier]
  public class CompactionExportController : BaseController<CompactionExportController>
  {
    private readonly IASNodeClient raptorClient;
    private readonly IPreferenceProxy prefProxy;
    private readonly IProductionDataRequestFactory requestFactory;
    
    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    private readonly ITRexCompactionDataProxy TRexCompactionDataProxy;

    /// <summary>
    /// 
    /// Default constructor.
    /// </summary>
    public CompactionExportController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IPreferenceProxy prefProxy, ITRexCompactionDataProxy trexCompactionDataProxy) :
      base(configStore, fileListProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.prefProxy = prefProxy;
      this.requestFactory = requestFactory;
      TRexCompactionDataProxy = trexCompactionDataProxy;
    }

    #region Schedule Exports

    /// <summary>
    /// Schedules the veta export job and returns JobId.
    /// </summary>
    [Route("api/v2/export/veta/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleVetaJob(
      [FromServices] ISchedulerProxy scheduler,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid,
      [FromQuery] CoordType coordType = CoordType.Northeast)
    {
      //TODO: Do we need to validate the parameters here as well as when the export url is called?

      //The URL to get the export data is here in this controller, construct it based on this request
      var exportDataUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v2/export/veta?projectUid={projectUid}&fileName={fileName}&coordType={coordType}";
      
      if (filterUid.HasValue)
      {
        exportDataUrl = $"{exportDataUrl}&filterUid={filterUid}";
      }
      if (!string.IsNullOrEmpty(machineNames))
      {
        exportDataUrl = $"{exportDataUrl}&machineNames={machineNames}";
      }

      return ScheduleJob(exportDataUrl, fileName, scheduler);
    }

    /// <summary>
    /// Schedules the Machine passes export job and returns JobId.
    /// </summary>
    [Route("api/v2/export/machinepasses/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleMachinePassesJob(
      [FromQuery] Guid projectUid,
      [FromQuery] int coordType,
      [FromQuery] int outputType,
      [FromQuery] bool restrictOutput,
      [FromQuery] bool rawDataOutput,
      [FromQuery] string fileName,
      [FromQuery] Guid? filterUid,
      [FromServices] ISchedulerProxy scheduler)
    {
      //TODO: Do we need to validate the parameters here as well as when the export url is called?

      //The URL to get the export data is here in this controller, construct it based on this request
      var exportDataUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v2/export/machinepasses?projectUid={projectUid}&fileName={fileName}&filterUid={filterUid}" +
                          $"&coordType={coordType}&outputType={outputType}&restrictOutput={restrictOutput}&rawDataOutput={rawDataOutput}";
      return ScheduleJob(exportDataUrl, fileName, scheduler);
    }

    /// <summary>
    /// Schedules the surface export job and returns JobId.
    /// </summary>
    [Route("api/v2/export/surface/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleSurfaceJob(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid,
      [FromServices] ISchedulerProxy scheduler)
    {
      //TODO: Do we need to validate the parameters here as well as when the export url is called?

      var exportDataUrl =
        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v2/export/surface?projectUid={projectUid}&fileName={fileName}&filterUid={filterUid}&tolerance={tolerance}";
      return ScheduleJob(exportDataUrl, fileName, scheduler);
    }

    /// <summary>
    /// Schedule an export job wit the scheduler
    /// </summary>
    private ScheduleResult ScheduleJob(string exportDataUrl, string fileName, ISchedulerProxy scheduler)
    {
      var timeout = ConfigStore.GetValueInt("SCHEDULED_JOB_TIMEOUT");
      if (timeout == 0) timeout = 300000;//5 mins default
      var request = new ScheduleJobRequest { Url = exportDataUrl, Filename = fileName, Timeout = timeout };

      return WithServiceExceptionTryExecute(() => new ScheduleResult
      {
        JobId = scheduler.ScheduleExportJob(request, Request.Headers.GetCustomHeaders(true)).Result?.JobId
      });
    }

    #endregion

    #region Exports

    /// <summary>
    /// Gets an export of production data in cell grid format report for import to VETA.
    /// </summary>
    [Route("api/v2/export/veta")]
    [HttpGet]
    public async Task<FileResult> GetExportReportVeta(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid,
      [FromQuery] CoordType coordType = CoordType.Northeast)
    {
      Log.LogInformation($"{nameof(GetExportReportVeta)}: {Request.QueryString}");

      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();
      var startEndDate = GetDateRange(project.LegacyProjectId, filter);

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(project.LegacyProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetRaptorClient(raptorClient)
        .SetUserPreferences(userPreferences)
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          startEndDate.Item1,
          startEndDate.Item2,
          coordType,
          ExportTypes.VedaExport,
          fileName,
          false,
          true,
          OutputTypes.VedaAllPasses,
          machineNames);

      exportRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, ConfigStore)
          .Process(exportRequest) as CompactionExportResult);

      var fileStream = new FileStream(result.FullFileName, FileMode.Open);
      Log.LogInformation($"GetExportReportVeta completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, "application/zip");
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
    [Route("api/v2/export/machinepasses")]
    [HttpGet]
    public async Task<FileResult> GetExportReportMachinePasses(
      [FromQuery] Guid projectUid,
      [FromQuery] int coordType,
      [FromQuery] int outputType,
      [FromQuery] bool restrictOutput,
      [FromQuery] bool rawDataOutput,
      [FromQuery] string fileName,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetExportReportMachinePasses: " + Request.QueryString);

      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();
      var startEndDate = GetDateRange(project.LegacyProjectId, filter);

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(project.LegacyProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          startEndDate.Item1,
          startEndDate.Item2,
          (CoordType)coordType,
          ExportTypes.PassCountExport,
          fileName,
          restrictOutput,
          rawDataOutput,
          (OutputTypes)outputType,
          string.Empty);

      exportRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, ConfigStore)
          .Process(exportRequest) as CompactionExportResult);

      var fileStream = new FileStream(result.FullFileName, FileMode.Open);
      Log.LogInformation($"GetExportReportMachinePasses completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, "application/zip");
    }

    /// <summary>
    /// Gets an export of 3D project data in .TTM file format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="tolerance">Controls triangulation density in the output .TTM file.</param>
    /// <param name="filterUid">The filter Uid to apply to the export results</param>
    [Route("api/v2/export/surface")]
    [HttpGet]
    public async Task<FileResult> GetExportReportSurface(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid)
    {
      const double surfaceExportTollerance = 0.05;

      Log.LogInformation("GetExportReportSurface: " + Request.QueryString);

      var project = await ((RaptorPrincipal) User).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();

      tolerance = tolerance ?? surfaceExportTollerance;

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(project.LegacyProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          null, //startUtc,
          null, //endUtc,
          CoordType.Northeast,
          ExportTypes.SurfaceExport,
          fileName,
          false,
          false,
          OutputTypes.VedaAllPasses,
          string.Empty,
          tolerance.Value);

      exportRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, configStore: ConfigStore,
            trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
          .Process(exportRequest) as CompactionExportResult);

      var fileStream = new FileStream(result.FullFileName, FileMode.Open);
      
      Log.LogInformation($"GetExportReportSurface completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, "application/zip");
    }
    #endregion

    /// <summary>
    /// Get user preferences
    /// </summary>
    private async Task<UserPreferenceData> GetUserPreferences()
    {
      var userPreferences = await prefProxy.GetUserPreferences(CustomHeaders);
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
    private Tuple<DateTime, DateTime> GetDateRange(long projectId, FilterResult filter)
    {
      if (filter?.StartUtc == null || !filter.EndUtc.HasValue)
      {
        //Special case of project extents where start and end UTC not set in filter for Raptor peformance.
        //But need to set here for export.
        var excludedIds = filter?.SurveyedSurfaceExclusionList?.ToArray() ?? new long[0];
        ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, excludedIds);
        request.Validate();

        var result =
          RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(LoggerFactory, raptorClient)
            .Process(request) as ProjectStatisticsResult;

        var startUtc = filter?.StartUtc ?? result.startTime;
        var endUtc = filter?.EndUtc ?? result.endTime;
        return new Tuple<DateTime, DateTime>(startUtc, endUtc);
      }

      return new Tuple<DateTime, DateTime>(filter.StartUtc.Value, filter.EndUtc.Value);
    }
  }
}
