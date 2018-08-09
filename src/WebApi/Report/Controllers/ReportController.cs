using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Contracts;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Report.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ReportController : IReportSvc
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="configStore">Configuration store</param>
    public ReportController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.configStore = configStore;
    }

    /// <summary>
    /// Pings the export service root
    /// </summary>
    /// <returns></returns>
    [Route("api/v1/export/ping")]
    [HttpPost]
    public string PostExportCSVReport()
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
    public ExportResult PostExportCSVReport([FromBody] ExportGridCSV request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<ExportGridCSVExecutor>(logger, raptorClient, null, configStore).Process(request) as ExportResult;
    }

    [PostRequestVerifier]
    [Route("api/v1/export")]
    [HttpPost]

    public ExportResult PostExportReport([FromBody] ExportReport request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<ExportReportExecutor>(logger, raptorClient, null, configStore)
          .Process(request) as ExportResult;
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
    public PassCountSummaryResult PostSummary([FromBody] PassCounts request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<SummaryPassCountsExecutor>(logger, raptorClient).Process(request)
          as PassCountSummaryResult;
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
    public PassCountDetailedResult PostDetailed([FromBody] PassCounts request)
    {
      request.Validate();
      //pass count settings required for detailed report
      if (request.passCountSettings == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Pass count settings required for detailed pass count report"));
      }
      return
        RequestExecutorContainerFactory.Build<DetailedPassCountExecutor>(logger, raptorClient).Process(request)
          as PassCountDetailedResult;
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
    public CMVSummaryResult PostSummary([FromBody] CMVRequest request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<SummaryCMVExecutor>(logger, raptorClient).Process(request) as
          CMVSummaryResult;
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
    public CMVDetailedResult PostDetailed([FromBody] CMVRequest request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<DetailedCMVExecutor>(logger, raptorClient).Process(request) as
          CMVDetailedResult;

    }

    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    /// <param name="request">The request for statistics request to Raptor</param>
    /// <returns></returns>
    /// <executor>ProjectStatisticsExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/projects/statistics")]
    [HttpPost]
    public ProjectStatisticsResult PostProjectStatistics([FromBody] ProjectStatisticsRequest request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient).Process(request)
          as ProjectStatisticsResult;
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
    public SummaryVolumesResult Post([FromBody] SummaryVolumesRequest request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<SummaryVolumesExecutor>(logger, raptorClient).Process(request) as
          SummaryVolumesResult;
    }

    /// <summary>
    /// Gets Thickness summary from Raptor.
    /// </summary>
    /// <param name="parameters">The request for thickness summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>SummaryVolumesExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/thickness/summary")]
    [HttpPost]
    public SummaryThicknessResult Post([FromBody] SummaryParametersBase parameters)
    {
      parameters.Validate();
      return
        RequestExecutorContainerFactory.Build<SummaryThicknessExecutor>(logger, raptorClient).Process(parameters)
          as SummaryThicknessResult;
    }

    /// <summary>
    /// Gets Speed summary from Raptor.
    /// </summary>
    /// <param name="parameters">The request for speed summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>SummarySpeedExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/speed/summary")]
    [HttpPost]
    public SpeedSummaryResult Post([FromBody] SummarySpeedRequest parameters)
    {
      parameters.Validate();
      return
        RequestExecutorContainerFactory.Build<SummarySpeedExecutor>(logger, raptorClient).Process(parameters) as
          SpeedSummaryResult;
    }

    /// <summary>
    /// Gets CMV Change summary from Raptor. This request uses absolute values of CMV.
    /// </summary>
    /// <param name="parameters">The request for CMV Change summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>CMVChangeSummaryExecutor</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/cmvchange/summary")]
    [HttpPost]
    public CMVChangeSummaryResult Post([FromBody] CMVChangeSummaryRequest parameters)
    {
      parameters.Validate();
      return
        RequestExecutorContainerFactory.Build<CMVChangeSummaryExecutor>(logger, raptorClient).Process(parameters)
          as CMVChangeSummaryResult;
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
    public ElevationStatisticsResult Post([FromBody] ElevationStatisticsRequest request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<ElevationStatisticsExecutor>(logger, raptorClient).Process(request)
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
    public CCASummaryResult PostSummary([FromBody] CCARequest request)
    {
      request.Validate();
      return
        RequestExecutorContainerFactory.Build<SummaryCCAExecutor>(logger, raptorClient).Process(request) as
          CCASummaryResult;
    }
  }
}
