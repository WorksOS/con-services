using System.Net;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Contracts;
using VSS.Raptor.Service.WebApiModels.Report.Executors;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApi.Report.Controllers
{
    public class ReportController : Controller, IReportSvc
    {
        /// <summary>
        /// Raptor client for use by executor
        /// </summary>
        private readonly IASNodeClient raptorClient;

        /// <summary>
        /// Logger for logging
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// Logger factory for use by executor
        /// </summary>
        private readonly ILoggerFactory logger;

        private IConfigurationStore configStore;

        /// <summary>
        /// Constructor with injected raptor client and logger
        /// </summary>
        /// <param name="raptorClient">Raptor client</param>
        /// <param name="logger">Logger</param>
        public ReportController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore)
        {
            this.raptorClient = raptorClient;
            this.logger = logger;
            this.log = logger.CreateLogger<ReportController>();
            this.configStore = configStore;
        }


        #region PassCounts reports

        /*[ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]*/

        [Route("api/v1/export")]
        [HttpPost]

        public ExportResult PostExportReport([FromBody] ExportReport request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<ExportReportExecutor>(logger, raptorClient, null, configStore)
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
        [ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]
        [Route("api/v1/compaction/passcounts/summary")]
        [HttpPost]
        public PassCountSummaryResult PostSummary([FromBody] PassCounts request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<SummaryPassCountsExecutor>(logger, raptorClient, null).Process(request)
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
        [ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]
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
                RequestExecutorContainer.Build<DetailedPassCountExecutor>(logger, raptorClient, null).Process(request)
                    as PassCountDetailedResult;
        }

        #endregion

        #region CMV reports

        /// <summary>
        /// Posts summary CMV request to Raptor. 
        /// </summary>
        /// <param name="request">Summary CMV request request</param>
        /// <returns>Returns JSON structure wtih operation result.
        /// </returns>
        /// <executor>SummaryCMVExecutor</executor>
        [ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]
        [Route("api/v1/compaction/cmv/summary")]
        [HttpPost]
        public CMVSummaryResult PostSummary([FromBody] CMVRequest request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<SummaryCMVExecutor>(logger, raptorClient, null).Process(request) as
                    CMVSummaryResult;

        }

        /// <summary>
        /// Posts detailed CMV request to Raptor. 
        /// </summary>
        /// <param name="request">Detailed CMV request request</param>
        /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
        /// </returns>
        /// <executor>DetailedCMVExecutor</executor>     
        [ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]
        [Route("api/v1/compaction/cmv/detailed")]
        [HttpPost]
        public CMVDetailedResult PostDetailed([FromBody] CMVRequest request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<DetailedCMVExecutor>(logger, raptorClient, null).Process(request) as
                    CMVDetailedResult;

        }

        #endregion




        /// <summary>
        /// Gets project statistics from Raptor.
        /// </summary>
        /// <param name="request">The request for statistics request to Raptor</param>
        /// <returns></returns>
        /// <executor>ProjectStatisticsExecutor</executor>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v1/projects/statistics")]
        [HttpPost]
        public ProjectStatisticsResult PostProjectStatistics([FromBody] ProjectStatisticsRequest request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<ProjectStatisticsExecutor>(logger, raptorClient, null).Process(request)
                    as ProjectStatisticsResult;
        }

    // TEMP v2 copy of v1 until we have a simplified contract for Compaction
    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    /// <param name="request">The request for statistics request to Raptor</param>
    /// <returns></returns>
    /// <executor>ProjectStatisticsExecutor</executor>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/projects/statistics")]
    [HttpPost]
    public ProjectStatisticsResult PostProjectStatistics2([FromBody] ProjectStatisticsRequest request)
    {
      request.Validate();
      return
          RequestExecutorContainer.Build<ProjectStatisticsExecutor>(logger, raptorClient, null).Process(request)
              as ProjectStatisticsResult;
    }



    /// <summary>
    /// Gets volumes summary from Raptor.
    /// </summary>
    /// <param name="request">The request for volumes summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>SummaryVolumesExecutor</executor>
    [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v1/volumes/summary")]
        [HttpPost]
        public SummaryVolumesResult Post([FromBody] SummaryVolumesRequest request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<SummaryVolumesExecutor>(logger, raptorClient).Process(request) as
                    SummaryVolumesResult;
        }

        /// <summary>
        /// Gets Thickness summary from Raptor.
        /// </summary>
        /// <param name="parameters">The request for thickness summary request to Raptor</param>
        /// <returns></returns>
        /// <executor>SummaryVolumesExecutor</executor>
        [ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]
        [Route("api/v1/thickness/summary")]
        [HttpPost]
        public SummaryThicknessResult Post([FromBody] SummaryParametersBase parameters)
        {
            parameters.Validate();
            return
                RequestExecutorContainer.Build<SummaryThicknessExecutor>(logger, raptorClient, null).Process(parameters)
                    as SummaryThicknessResult;
        }


        /// <summary>
        /// Gets Speed summary from Raptor.
        /// </summary>
        /// <param name="parameters">The request for speed summary request to Raptor</param>
        /// <returns></returns>
        /// <executor>SummarySpeedExecutor</executor>
        [ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]
        [Route("api/v1/speed/summary")]
        [HttpPost]
        public SummarySpeedResult Post([FromBody] SummarySpeedRequest parameters)
        {
            parameters.Validate();
            return
                RequestExecutorContainer.Build<SummarySpeedExecutor>(logger, raptorClient, null).Process(parameters) as
                    SummarySpeedResult;
        }


        /// <summary>
        /// Gets CMV Change summary from Raptor. This request uses absolute values of CMV.
        /// </summary>
        /// <param name="parameters">The request for CMV Change summary request to Raptor</param>
        /// <returns></returns>
        /// <executor>CMVChangeSummaryExecutor</executor>
        [ProjectIdVerifier]
        [NotLandFillProjectVerifier]
        [ProjectUidVerifier]
        [NotLandFillProjectWithUIDVerifier]
        [Route("api/v1/cmvchange/summary")]
        [HttpPost]
        public CMVChangeSummaryResult Post([FromBody] CMVChangeSummaryRequest parameters)
        {
            parameters.Validate();
            return
                RequestExecutorContainer.Build<CMVChangeSummaryExecutor>(logger, raptorClient, null).Process(parameters)
                    as CMVChangeSummaryResult;
        }

        /// <summary>
        /// Gets elevation statistics from Raptor.
        /// </summary>
        /// <param name="request">The request for elevation statistics request to Raptor</param>
        /// <returns></returns>
        /// <executor>ElevationStatisticsExecutor</executor>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v1/statistics/elevation")]
        [HttpPost]
        public ElevationStatisticsResult Post([FromBody] ElevationStatisticsRequest request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<ElevationStatisticsExecutor>(logger, raptorClient, null).Process(request)
                    as ElevationStatisticsResult;
        }

        /// <summary>
        /// Posts summary CCA request to Raptor. 
        /// </summary>
        /// <param name="request">Summary CCA request</param>
        /// <returns>Returns JSON structure wtih operation result.
        /// </returns>
        /// <executor>SummaryCCAExecutor</executor>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v1/compaction/cca/summary")]
        [HttpPost]
        public CCASummaryResult PostSummary([FromBody] CCARequest request)
        {
            request.Validate();
            return
                RequestExecutorContainer.Build<SummaryCCAExecutor>(logger, raptorClient, null).Process(request) as
                    CCASummaryResult;

        }
    }
}
