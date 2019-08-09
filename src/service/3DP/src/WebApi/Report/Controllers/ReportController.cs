using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Report.Contracts;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Report.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ReportController : Controller, IReportSvc
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
    private readonly ILoggerFactory logger;
    private ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    private readonly ITRexCompactionDataProxy tRexCompactionDataProxy;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileImportProxy fileImportProxy;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    private IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    private string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="tRexCompactionDataProxy"></param>
    public ReportController(
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy tRexCompactionDataProxy, IFileImportProxy fileImportProxy)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this.logger = logger;
      log = logger.CreateLogger<ReportController>();
      this.configStore = configStore;
      this.tRexCompactionDataProxy = tRexCompactionDataProxy;
      this.fileImportProxy = fileImportProxy;
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
    public ExportResult PostExportCsvReport([FromBody] ExportGridCSV request)
    {
      log.LogDebug($"{nameof(PostExportCsvReport)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
#if RAPTOR
      return RequestExecutorContainerFactory.Build<ExportGridCSVExecutor>(logger, raptorClient, null, configStore).Process(request) as ExportResult;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    [PostRequestVerifier]
    [Route("api/v1/export")]
    [HttpPost]
    public async Task<ExportResult> PostExportReport([FromBody] ExportReport request)
    {
      log.LogDebug($"{nameof(PostExportReport)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();

      return await RequestExecutorContainerFactory.Build<ExportReportExecutor>(
          logger,
#if RAPTOR
          raptorClient,
          null,
#endif
          configStore: configStore,
          trexCompactionDataProxy: tRexCompactionDataProxy)
          .ProcessAsync(request) as ExportResult;
    }

    /// <summary>
    /// Posts summary pass count request to Raptor. 
    /// This is a summary of whether the pass count exceeds the target, meets the pass count target, or falls below the target.
    /// </summary>
    /// <param name="request">Summary pass counts request request</param>
    /// <returns>Returns JSON structure wtih operation result.
    /// </returns>
    /// <executor>SummaryPassCountsExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/passcounts/summary")]
    [HttpPost]
    public async Task<PassCountSummaryResult> PostExportSummaryPasscounts([FromBody] PassCounts request)
    {
      log.LogDebug($"{nameof(PostExportSummaryPasscounts)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<SummaryPassCountsExecutor>(logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore,
            trexCompactionDataProxy: tRexCompactionDataProxy,
            customHeaders: CustomHeaders)
            .ProcessAsync(request) as PassCountSummaryResult;
    }

    /// <summary>
    /// Posts detailed pass count request to Raptor. 
    /// This is the number of machine passes over a cell.
    /// </summary>
    /// <param name="request">Detailed pass counts request request</param>
    /// <returns>Returns JSON structure with operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>DetailedPassCountExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/passcounts/detailed")]
    [HttpPost]
    public async Task<PassCountDetailedResult> PostExportDetailedPasscounts([FromBody] PassCounts request)
    {
      log.LogDebug($"{nameof(PostExportDetailedPasscounts)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      //pass count settings required for detailed report
      if (request.passCountSettings == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Pass count settings required for detailed pass count report"));
      }
      return await RequestExecutorContainerFactory.Build<DetailedPassCountExecutor>(logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders)
            .ProcessAsync(request) as PassCountDetailedResult;
    }

    /// <summary>
    /// Posts summary CMV request to Raptor. 
    /// </summary>
    /// <param name="request">Summary CMV request request</param>
    /// <returns>Returns JSON structure wtih operation result.
    /// </returns>
    /// <executor>SummaryCMVExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/cmv/summary")]
    [HttpPost]
    public async Task<CMVSummaryResult> PostExportSummaryCmv([FromBody] CMVRequest request)
    {
      log.LogDebug($"{nameof(PostExportSummaryCmv)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<SummaryCMVExecutor>(logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders)
            .ProcessAsync(request) as CMVSummaryResult;
    }

    /// <summary>
    /// Posts detailed CMV request to Raptor. 
    /// </summary>
    /// <param name="request">Detailed CMV request request</param>
    /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>DetailedCMVExecutor</executor>     
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/cmv/detailed")]
    [HttpPost]
    public async Task<CMVDetailedResult> PostExportDetailedCmv([FromBody] CMVRequest request)
    {
      log.LogDebug($"{nameof(PostExportDetailedCmv)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<DetailedCMVExecutor>(logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders)
            .ProcessAsync(request) as CMVDetailedResult;
    }

    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    /// <param name="request">The request for statistics request to Raptor</param>
    /// <returns></returns>
    /// <executor>ProjectStatisticsExecutor</executor>
    [Obsolete("We now use CompactionElevationController api/v2/projectstatistics")]
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/projects/statistics")]
    [HttpPost]
    public async Task<ProjectStatisticsResult> PostProjectStatistics([FromBody] ProjectStatisticsRequest request)
    {
      log.LogDebug($"{nameof(PostProjectStatistics)}: {JsonConvert.SerializeObject(request)}");

      if (!request.ProjectUid.HasValue)
        request.ProjectUid = await ((RaptorPrincipal) User).GetProjectUid(request.ProjectId ?? -1);

      request.Validate();

      var projectStatisticsHelper = new ProjectStatisticsHelper(logger, configStore, fileImportProxy, tRexCompactionDataProxy
#if RAPTOR
        , raptorClient
#endif
        );

      return await projectStatisticsHelper.GetProjectStatisticsWithFilterSsExclusions(
        request.ProjectUid ?? Guid.Empty, 
        request.ProjectId ?? -1,
        request.ExcludedSurveyedSurfaceIds?.ToList() ?? new List<long>(0), 
        GetUserId(), CustomHeaders);
    }

    /// <summary>
    /// Gets volumes summary from Raptor.
    /// </summary>
    /// <param name="request">The request for volumes summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>SummaryVolumesExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/volumes/summary")]
    [HttpPost]
    public SummaryVolumesResult PostExportSummaryVolumes([FromBody] SummaryVolumesRequest request)
    {
      log.LogDebug($"{nameof(PostExportSummaryVolumes)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
#if RAPTOR
      return
        RequestExecutorContainerFactory.Build<SummaryVolumesExecutor>(logger, raptorClient).Process(request) as
          SummaryVolumesResult;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
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
      log.LogDebug($"{nameof(PostExportSummaryThickness)}: {JsonConvert.SerializeObject(request)}");

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

    /// <summary>
    /// Gets Speed summary from Raptor.
    /// </summary>
    /// <param name="request">The request for speed summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>SummarySpeedExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/speed/summary")]
    [HttpPost]
    public async Task<SpeedSummaryResult> PostExportSummarySpeed([FromBody] SummarySpeedRequest request)
    {
      log.LogDebug($"{nameof(PostExportSummarySpeed)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<SummarySpeedExecutor>(logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders)
            .ProcessAsync(request) as SpeedSummaryResult;
    }

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
    public async Task<CMVChangeSummaryResult> PostExportSummaryCmvChange([FromBody] CMVChangeSummaryRequest request)
    {
      log.LogDebug($"{nameof(PostExportSummaryCmvChange)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      return await RequestExecutorContainerFactory.Build<CMVChangeSummaryExecutor>(logger,
#if RAPTOR
            raptorClient, 
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders)
            .ProcessAsync(request) as CMVChangeSummaryResult;
    }

    /// <summary>
    /// Gets elevation statistics from Raptor.
    /// </summary>
    /// <param name="request">The request for elevation statistics request to Raptor</param>
    /// <returns></returns>
    /// <executor>ElevationStatisticsExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/statistics/elevation")]
    [HttpPost]
    public async Task<ElevationStatisticsResult> PostExportElevationStatistics([FromBody] ElevationStatisticsRequest request)
    {
      log.LogDebug($"{nameof(PostExportElevationStatistics)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();

      return
        await RequestExecutorContainerFactory.Build<ElevationStatisticsExecutor>(logger,
#if RAPTOR
            raptorClient,
#endif
            configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request)
          as ElevationStatisticsResult;
    }

    /// <summary>
    /// Posts summary CCA request to Raptor. 
    /// </summary>
    /// <param name="request">Summary CCA request</param>
    /// <returns>Returns JSON structure wtih operation result.
    /// </returns>
    /// <executor>SummaryCCAExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/compaction/cca/summary")]
    [HttpPost]
    public async Task<CCASummaryResult> PostExportCcaSummary([FromBody] CCARequest request)
    {
      log.LogDebug($"{nameof(PostExportCcaSummary)}: {JsonConvert.SerializeObject(request)}");

      if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_CCA") ?? false)
        request.ProjectUid = await GetProjectUid(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID);

      request.Validate();

      return await RequestExecutorContainerFactory.Build<SummaryCCAExecutor>(logger,
#if RAPTOR
            raptorClient, 
#endif
          configStore: configStore, trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as
          CCASummaryResult;
    }
  }
}
