using Microsoft.AspNetCore.Mvc;
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
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
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
    /// <param name="raptorClient">The raptor client.</param>
    /// <param name="loggerFactory">The loggerFactory.</param>
    /// <param name="configStore">Configuration store</param>/// 
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    /// <param name="prefProxy">User preferences proxy</param>
    /// <param name="transferProxy">Export file download proxy</param>
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
      const double surfaceExportTollerance = 0.05;

      Log.LogInformation("GetExportReportSurface: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();

      tolerance = tolerance ?? surfaceExportTollerance;

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
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as ExportResult
      );
    }

    /// <summary>
    /// Tries to get export status.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <exception cref="ServiceException">new ContractExecutionResult(-4,"Job failed for some reason")</exception>
    /// <exception cref="ContractExecutionResult">-4 - Job failed for some reason</exception>
    [ProjectUidVerifier]
    [Route("api/v2/export/veta/status")]
    [HttpGet]
    public async Task<ContractExecutionResult> TryGetExportStatus([FromQuery] Guid projectUid, [FromQuery] string jobId,
      [FromServices] ISchedulerProxy scheduler)
    {
      var jobResult = await scheduler.GetVetaExportJobStatus(projectUid, jobId, Request.Headers.GetCustomHeaders(true));
      if (jobResult.status.Equals("SUCCEEDED", StringComparison.OrdinalIgnoreCase))
      {
        return new ContractExecutionResult();
      }
      if (!jobResult.status.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.PartialData, "Job is running");
      }
      throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Job failed for some reason"));
    }

    /// <summary>
    /// Tries the download of the exported file.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <exception cref="ServiceException">new ContractExecutionResult(-4, "File is not likely ready to be downloaded")</exception>
    /// <exception cref="ContractExecutionResult">-4 - File is not likely ready to be downloaded</exception>
    [ProjectUidVerifier]
    [Route("api/v2/export/veta/download")]
    [HttpGet]
    public async Task<FileResult> TryDownload([FromQuery] Guid projectUid, [FromQuery] string jobId,
      [FromServices] ISchedulerProxy scheduler)
    {
      var jobResult = await scheduler.GetVetaExportJobStatus(projectUid, jobId, Request.Headers.GetCustomHeaders(true));

      if (jobResult.status.Equals("SUCCEEDED", StringComparison.OrdinalIgnoreCase))
      {
        return await transferProxy.Download(jobResult.key);
      }
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "File is likely not ready to be downloaded"));
    }

    /// <summary>
    /// Tries the download of the exported file. Used for acceptance tests.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <exception cref="ServiceException">new ContractExecutionResult(-4, "File is not likely ready to be downloaded")</exception>
    /// <exception cref="ContractExecutionResult">-4 - File is not likely ready to be downloaded</exception>
    [ProjectUidVerifier]
    [Route("api/v2/export/veta/downloadtest")]
    [HttpGet]
    public async Task<ExportResult> TryDownloadTest([FromQuery] Guid projectUid, [FromQuery] string jobId,
      [FromServices] ISchedulerProxy scheduler)
    {
      var result = await TryDownload(projectUid, jobId, scheduler) as FileStreamResult;

      using (var reader = new BinaryReader(result.FileStream))
      {
        return ExportResult.Create(reader.ReadBytes((int)result.FileStream.Length), 0);
      }
    }

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
    public ScheduleResult ScheduleVetaJob([FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid,
      [FromServices] ISchedulerProxy scheduler)
    {
      return
        WithServiceExceptionTryExecute(() => new ScheduleResult
        {
          JobId =
            scheduler.ScheduleVetaExportJob(projectUid, fileName, machineNames, filterUid,
              Request.Headers.GetCustomHeaders(true)).Result?.jobId
        });
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
      Log.LogInformation("GetExportReportVeta: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();
      var startEndDate = GetDateRange(projectId, filter);

      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetRaptorClient(raptorClient)
        .SetUserPreferences(userPreferences)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
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

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, this.ConfigStore)
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
      Log.LogInformation("GetExportReportMachinePasses: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var userPreferences = await GetUserPreferences();
      var startEndDate = GetDateRange(projectId, filter);

      var exportRequest = await requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor((User as RaptorPrincipal).GetProject(projectUid))
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

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, null, this.ConfigStore)
          .Process(exportRequest) as ExportResult
      );
    }

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
    private Tuple<DateTime, DateTime> GetDateRange(long projectId, Common.Models.Filter filter)
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

        return new Tuple<DateTime, DateTime>(result.startTime, result.endTime);
      }

      return new Tuple<DateTime, DateTime>(filter.StartUtc.Value, filter.EndUtc.Value);
    }
  }
}