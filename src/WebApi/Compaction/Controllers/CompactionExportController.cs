﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  //NOTE: do not cache responses as large amount of data
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CompactionExportController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private readonly IPreferenceProxy prefProxy;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    private readonly ITransferProxy transferProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionExportController(IASNodeClient raptorClient, ILoggerFactory loggerFactory, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy,
      IPreferenceProxy prefProxy, ITransferProxy transferProxy) :
      base(loggerFactory, loggerFactory.CreateLogger<CompactionExportController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.prefProxy = prefProxy;
      this.requestFactory = requestFactory;
      this.transferProxy = transferProxy;
    }

    #region Schedule Veta Export
    /// <summary>
    /// Schedules the veta export job and returns JobId.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="machineNames">The machine names.</param>
    /// <param name="filterUid">The filter uid.</param>
    /// <param name="scheduler">The scheduler.</param>
    [ProjectUidVerifier]
    [Route("api/v2/export/veta/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleVetaJob(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid,
      [FromServices] ISchedulerProxy scheduler)
    {
      //TODO: Do we need to validate the parameters here as well as when the export url is called?

      //The URL to get the export data is here in this controller, construct it based on this request
      var exportDataUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v2/export/veta?projectUid={projectUid}&fileName={fileName}";
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
    #endregion

    #region Schedule Machine passes Export

    /// <summary>
    /// Schedules the Machine passes export job and returns JobId.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="rawDataOutput"></param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="filterUid">The filter uid.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="coordType"></param>
    /// <param name="outputType"></param>
    /// <param name="restrictOutput"></param>
    [ProjectUidVerifier]
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
    #endregion

    #region Schedule surface export

    /// <summary>
    /// Schedules the surface export job and returns JobId.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="tolerance"></param>
    /// <param name="filterUid">The filter uid.</param>
    /// <param name="scheduler">The scheduler.</param>
    [ProjectUidVerifier]
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
      return
        WithServiceExceptionTryExecute(() => new ScheduleResult
        {
          JobId =
            scheduler.ScheduleExportJob(request, Request.Headers.GetCustomHeaders(true)).Result?.JobId
        });
    }
    #endregion


    #region Exports
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
    public async Task<FileResult> GetExportReportVeta(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetExportReportVeta: " + Request.QueryString);

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing export file name"));
      }

      var project = await (User as RaptorPrincipal).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();
      var startEndDate = GetDateRange(project.LegacyProjectId, filter);

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(project.LegacyProjectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetRaptorClient(raptorClient)
        .SetUserPreferences(userPreferences)
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          startEndDate.Item1,
          startEndDate.Item2,
          CoordTypes.ptNORTHEAST,
          ExportTypes.kVedaExport,
          fileName,
          false,
          true,
          OutputTypes.etVedaAllPasses,
          machineNames);

      exportRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as CompactionExportResult
      );

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
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
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

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing export file name"));
      }

      var project = await (User as RaptorPrincipal).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();
      var startEndDate = GetDateRange(project.LegacyProjectId, filter);

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(project.LegacyProjectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          startEndDate.Item1,
          startEndDate.Item2,
          (CoordTypes)coordType,
          ExportTypes.kPassCountExport,
          fileName,
          restrictOutput,
          rawDataOutput,
          (OutputTypes)outputType,
          string.Empty);

      exportRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as CompactionExportResult
      );

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
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/surface")]
    [HttpGet]
    public async Task<FileResult> GetExportReportSurface(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid)
    {
      const double surfaceExportTollerance = 0.05;

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing export file name"));
      }

      Log.LogInformation("GetExportReportSurface: " + Request.QueryString);

      var project = await (User as RaptorPrincipal).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();

      tolerance = tolerance ?? surfaceExportTollerance;

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(project.LegacyProjectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor(project)
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

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as CompactionExportResult
      );

      var fileStream = new FileStream(result.FullFileName, FileMode.Open);
      Log.LogInformation($"GetExportReportSurface completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, "application/zip");
    }
    #endregion

    #region privates
    /// <summary>
    /// Get user preferences
    /// </summary>
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

        var startUtc = filter?.StartUtc == null ? result.startTime : filter.StartUtc.Value;
        var endUtc = filter?.EndUtc == null ? result.endTime : filter.EndUtc.Value;
        return new Tuple<DateTime, DateTime>(startUtc, endUtc);
      }

      return new Tuple<DateTime, DateTime>(filter.StartUtc.Value, filter.EndUtc.Value);
    }
    #endregion
  }
}
