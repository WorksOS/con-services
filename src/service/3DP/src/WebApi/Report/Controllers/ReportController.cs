using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Compaction.Controllers;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Report.Contracts;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Report.Controllers
{
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ReportController : BaseController<ReportController>, IReportSvc
  {
#if RAPTOR
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;
#endif
    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory _logger;
    private readonly ILogger _log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    private readonly ITRexCompactionDataProxy tRexCompactionDataProxy;
  

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="tRexCompactionDataProxy"></param>
    public ReportController(
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      ILoggerFactory logger, IConfigurationStore configStore, ICompactionSettingsManager settingsManager, ITRexCompactionDataProxy tRexCompactionDataProxy, IFileImportProxy fileImportProxy)
      : base(configStore, fileImportProxy, settingsManager)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this._logger = logger;
      _log = logger.CreateLogger<ReportController>();
      this.configStore = configStore;
      this.tRexCompactionDataProxy = tRexCompactionDataProxy;
    }

    /// <summary>
    /// Returns the ProjectUid (Guid) for a given ProjectId (long).
    /// </summary>
    protected Task<Guid> GetProjectUid(long projectId)
    {
      return ((RaptorPrincipal)User).GetProjectUid(projectId);
    }


    /// <summary>
    /// Pings the export service root
    /// </summary>
    /// <returns></returns>
    [Route("api/v1/export/ping")]
    [HttpPost]
    public string PostExportCsvReport()
    {
      return "Ping!";
    }

    /// <summary>
    /// Produces a CSV formatted export of production data identified by gridded sampling
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/export/gridded/csv")]
    [HttpPost]
    public async Task<ExportResult> PostExportCsvReport([FromBody] ExportGridCSV request)
    {
      _log.LogDebug($"{nameof(PostExportCsvReport)}: {JsonConvert.SerializeObject(request)}");

      if (request.liftBuildSettings == null)
      {
        var projectSettings = await GetProjectSettingsTargets(request.ProjectUid.Value);
        request.liftBuildSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);
      }
      
      request.Validate();

      return await RequestExecutorContainerFactory.Build<ExportGridCSVExecutor>(
        _logger,
        configStore: configStore,
        trexCompactionDataProxy: tRexCompactionDataProxy,
        userId: GetUserId(), fileImportProxy: FileImportProxy).ProcessAsync(request) as ExportResult;
    }

    [PostRequestVerifier]
    [Route("api/v1/export")]
    [HttpPost]
    public async Task<ExportResult> PostExportReport([FromBody] ExportReport request)
    {
      _log.LogDebug($"{nameof(PostExportReport)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();

      return await RequestExecutorContainerFactory.Build<ExportReportExecutor>(
          _logger,
#if RAPTOR
          raptorClient,
          null,
#endif
          configStore: configStore,
          trexCompactionDataProxy: tRexCompactionDataProxy,
          userId: GetUserId(), fileImportProxy: FileImportProxy)
          .ProcessAsync(request) as ExportResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Posts summary pass count request to Raptor. 
    /// This is a summary of whether the pass count exceeds the target, meets the pass count target, or falls below the target.
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/passcounts/summary")]
    [HttpPost]
    public async Task<PassCountSummaryResult> PostExportSummaryPasscountsTbc([FromBody] PassCounts request)
    {
      _log.LogDebug($"{nameof(PostExportSummaryPasscountsTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<SummaryPassCountsExecutor>(_logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore,
            trexCompactionDataProxy: tRexCompactionDataProxy, 
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
            .ProcessAsync(request) as PassCountSummaryResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Posts detailed pass count request to Raptor. 
    /// This is the number of machine passes over a cell.
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/passcounts/detailed")]
    [HttpPost]
    public async Task<PassCountDetailedResult> PostExportDetailedPasscountsTbc([FromBody] PassCounts request)
    {
      _log.LogDebug($"{nameof(PostExportDetailedPasscountsTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      //pass count settings required for detailed report
      if (request.passCountSettings == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Pass count settings required for detailed pass count report"));
      }
      return await RequestExecutorContainerFactory.Build<DetailedPassCountExecutor>(_logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, 
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
            .ProcessAsync(request) as PassCountDetailedResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Posts summary CMV request to Raptor. 
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/cmv/summary")]
    [HttpPost]
    public async Task<CMVSummaryResult> PostExportSummaryCmvTbc([FromBody] CMVRequest request)
    {
      _log.LogDebug($"{nameof(PostExportSummaryCmvTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<SummaryCMVExecutor>(_logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, 
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
            .ProcessAsync(request) as CMVSummaryResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Posts detailed CMV request to Raptor. 
    /// </summary>  
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/cmv/detailed")]
    [HttpPost]
    public async Task<CMVDetailedResult> PostExportDetailedCmvTbc([FromBody] CMVRequest request)
    {
      _log.LogDebug($"{nameof(PostExportDetailedCmvTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<DetailedCMVExecutor>(_logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, 
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
            .ProcessAsync(request) as CMVDetailedResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/projects/statistics")]
    [HttpPost]
    public async Task<ProjectStatisticsResult> PostProjectStatisticsTbc([FromBody] ProjectStatisticsRequest request)
    {
      _log.LogDebug($"{nameof(PostProjectStatisticsTbc)}: {JsonConvert.SerializeObject(request)}");

      if (!request.ProjectUid.HasValue)
        request.ProjectUid = await ((RaptorPrincipal) User).GetProjectUid(request.ProjectId ?? -1);

      request.Validate();

      var projectStatisticsHelper = new ProjectStatisticsHelper(_logger, configStore, FileImportProxy, tRexCompactionDataProxy
#if RAPTOR
        , raptorClient
#endif
        );

      return await projectStatisticsHelper.GetProjectStatisticsWithRequestSsExclusions(
        request.ProjectUid ?? Guid.Empty, 
        request.ProjectId ?? -1,
        GetUserId(),
        request.ExcludedSurveyedSurfaceIds,
        CustomHeaders);
    }

    /// Called by TBC only.
    /// <summary>
    /// Gets volumes summary from Raptor.
    /// </summary>
    /// <executor>SummaryVolumesExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/volumes/summary")]
    [HttpPost]
    public async Task<SummaryVolumesResult> PostExportSummaryVolumesTbc([FromBody] SummaryVolumesRequest request)
    {
      _log.LogDebug($"{nameof(PostExportSummaryVolumesTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await
        RequestExecutorContainerFactory.Build<SummaryVolumesExecutor>(_logger,
#if RAPTOR
            raptorClient,
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy,
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
            .ProcessAsync(request) as
          SummaryVolumesResult;
    }

    /// <summary>
    /// Gets Thickness summary from Raptor.
    /// </summary>
    /// <param name="request">The request for thickness summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>SummaryVolumesExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/thickness/summary")]
    [HttpPost]
    public SummaryThicknessResult PostExportSummaryThickness([FromBody] SummaryParametersBase request)
    {
      _log.LogDebug($"{nameof(PostExportSummaryThickness)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
#if RAPTOR
      return
        RequestExecutorContainerFactory.Build<SummaryThicknessExecutor>(logger, raptorClient).Process(request)
          as SummaryThicknessResult;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// Called by TBC only.
    /// <summary>
    /// Gets Speed summary from Raptor.
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/speed/summary")]
    [HttpPost]
    public async Task<SpeedSummaryResult> PostExportSummarySpeedTbc([FromBody] SummarySpeedRequest request)
    {
      _log.LogDebug($"{nameof(PostExportSummarySpeedTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<SummarySpeedExecutor>(_logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, 
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
            .ProcessAsync(request) as SpeedSummaryResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Gets CMV Change summary from Raptor. This request uses absolute values of CMV.
    /// </summary>
    /// <param name="request">The request for CMV Change summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>CMVChangeSummaryExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/cmvchange/summary")]
    [HttpPost]
    public async Task<CMVChangeSummaryResult> PostExportSummaryCmvChangeTbc([FromBody] CMVChangeSummaryRequest request)
    {
      _log.LogDebug($"{nameof(PostExportSummaryCmvChangeTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<CMVChangeSummaryExecutor>(_logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy,
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
            .ProcessAsync(request) as CMVChangeSummaryResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Gets elevation statistics from Raptor.
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/statistics/elevation")]
    [HttpPost]
    public async Task<ElevationStatisticsResult> PostExportElevationStatisticsTbc([FromBody] ElevationStatisticsRequest request)
    {
      _log.LogDebug($"{nameof(PostExportElevationStatisticsTbc)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();

      return
        await RequestExecutorContainerFactory.Build<ElevationStatisticsExecutor>(_logger,
#if RAPTOR
            raptorClient,
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy,
            fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId())
        .ProcessAsync(request)
          as ElevationStatisticsResult;
    }

    /// Called by TBC only.
    /// <summary>
    /// Posts summary CCA request to Raptor. 
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/cca/summary")]
    [HttpPost]
    public async Task<CCASummaryResult> PostExportCcaSummaryTbc([FromBody] CCARequest request)
    {
      _log.LogDebug($"{nameof(PostExportCcaSummaryTbc)}: {JsonConvert.SerializeObject(request)}");

      if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_CCA") ?? false)
        request.ProjectUid = await GetProjectUid(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID);

      request.Validate();

      return await RequestExecutorContainerFactory.Build<SummaryCCAExecutor>(_logger,
#if RAPTOR
            raptorClient, 
#endif
          configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy,
          fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId()).ProcessAsync(request) as
          CCASummaryResult;
    }
  }
}
