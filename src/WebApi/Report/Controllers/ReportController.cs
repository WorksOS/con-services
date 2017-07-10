using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using ASNode.ExportProductionDataCSV.RPC;
using BoundingExtents;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using VLPDDecls;
using VSS.GenericConfiguration;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Report.Contracts;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Report.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class ReportController : Controller, IReportSvc
  {
    #region privates
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

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    private readonly IConfigurationStore configStore;

    /// <summary>
    /// Creates an instance of the CMVRequest class and populate it with data.
    /// </summary>
    /// <returns></returns>
    private async Task<ExportReport> GetExportReportRequest(
      long? projectId, 
      Guid? projectUid, 
      DateTime? startUtc, 
      DateTime? endUtc, 
      CoordTypes coordType, 
      ExportTypes exportType,
      string fileName,
      bool restrictSize,
      bool rawData,
      OutputTypes outputType,
      string machineNames,
      double tolerance)
    {
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }

      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;

      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, Request.Headers.GetCustomHeaders());

      // Filter filter = CompactionSettings.CompactionFilter(startUtc, endUtc, null, null, null, null, this.GetMachines(assetId, machineName, isJohnDoe), null);
      Filter filter = CompactionSettings.CompactionFilter(null, null, null, null, null, null, null, excludedIds);

      T3DBoundingWorldExtent projectExtents = new T3DBoundingWorldExtent();
      TMachine[] machineList = null;

      if (exportType == ExportTypes.kSurfaceExport)
      {
        raptorClient.GetDataModelExtents(projectId.Value,
          RaptorConverters.convertSurveyedSurfaceExlusionList(excludedIds), out projectExtents);
      }
      else
      {
        TMachineDetail[] machineDetails = raptorClient.GetMachineIDs(projectId.Value);

        if (machineDetails != null)
        {
          //machineDetails = machineDetails.GroupBy(x => x.Name).Select(y => y.Last()).ToArray();

          if (machineNames != null)
          {
            var machineNamesArray = machineNames.Split(',');
            machineDetails = machineDetails.Where(machineDetail => machineNamesArray.Contains(machineDetail.Name)).ToArray();
          }
          
          machineList = machineDetails.Select(m => new TMachine() {AssetID = m.ID, MachineName = m.Name, SerialNo = ""}).ToArray();
        }
      }


      return ExportReport.CreateExportReportRequest(
        projectId.Value, 
        liftSettings, 
        filter, 
        -1, 
        null, 
        false, 
        null, 
        coordType, 
        startUtc ?? DateTime.MinValue,
        endUtc ?? DateTime.MinValue, 
        true, 
        tolerance, 
        false,
        restrictSize,
        rawData,
        projectExtents, 
        false,
        outputType,
        machineList,
        false,
        fileName, 
        exportType);
    }
    #endregion

    /// <summary>
    /// Constructor with injected raptor client and logger
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="configStore"></param>
    public ReportController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore, IFileListProxy fileListProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<ReportController>();
      this.configStore = configStore;
      this.fileListProxy = fileListProxy;
    }

    #region ExportPing
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
    #endregion

    #region CSVExport
    /// <summary>
    /// Produces a CSV formatted export of production data identified by gridded sampling
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// 
    [PostRequestVerifier]
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v1/export/gridded/csv")]
    [HttpPost]
    public ExportResult PostExportCSVReport([FromBody] ExportGridCSV request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<ExportGridCSVExecutor>(logger, raptorClient, null, configStore).Process(request) as ExportResult;
    }
    #endregion


    #region PassCounts reports

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// 
    [PostRequestVerifier]
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
    /// Gets an export of 3D project data in .TTM file format report.
    /// </summary>
    /// <returns></returns>
    /// 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/export/surface")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportSurface(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] string fileName,
      [FromQuery] double tolerance
      )
    {
      log.LogInformation("GetExportReportSurface: " + Request.QueryString);

      ExportReport request = await GetExportReportRequest(
        projectId,
        projectUid,
        null, //startUtc,
        null, //endUtc,
        CoordTypes.ptNORTHEAST,
        ExportTypes.kSurfaceExport,
        fileName,
        false,
        true,
        OutputTypes.etVedaAllPasses,
        "",
        tolerance);

      request.Validate();

      return RequestExecutorContainer.Build<ExportReportExecutor>(logger, raptorClient, null, configStore).Process(request) as ExportResult;
    }
    /*
        /// <summary>
        /// Gets an export of production data in cell grid format report.
        /// </summary>
        /// <returns></returns>
        /// 
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/export/machinepasses")]
        [HttpGet]
        public async Task<ExportResult> GetExportReportMachinePasses(
          [FromQuery] long? projectId,
          [FromQuery] Guid? projectUid,
          [FromQuery] DateTime? startUtc,
          [FromQuery] DateTime? endUtc,
          [FromQuery] string fileName,
          [FromQuery] long? assetId,
          [FromQuery] string machineName,
          [FromQuery] bool? isJohnDoe
        )
        {
          log.LogInformation("GetExportReportMachinePasses: " + Request.QueryString);

          ExportReport request = await GetExportReportRequest(
            projectId,
            projectUid,
            startUtc,
            endUtc,
            CoordTypes.ptNORTHEAST,
            ExportTypes.kPassCountExport,
            fileName,
            false,
            true,
            OutputTypes.etPassCountAllPasses,
            assetId,
            machineName,
            isJohnDoe);

          request.Validate();

          return RequestExecutorContainer.Build<ExportReportExecutor>(logger, raptorClient, null, configStore).Process(request) as ExportResult;
        }
    */
    /// <summary>
    /// Gets an export of production data in cell grid format report for import to VETA.
    /// </summary>
    /// <returns></returns>
    /// 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/export/veta")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportVeta(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] string fileName,
      [FromQuery] string machineNames)
    {
      log.LogInformation("GetExportReportVeta: " + Request.QueryString);

      ExportReport request = await GetExportReportRequest(
        projectId,
        projectUid,
        startUtc,
        endUtc,
        CoordTypes.ptNORTHEAST,
        ExportTypes.kVedaExport,
        fileName,
        false,
        true,
        OutputTypes.etVedaAllPasses,
        machineNames,
        0.0);

      request.Validate();

      return RequestExecutorContainer.Build<ExportReportExecutor>(logger, raptorClient, null, configStore).Process(request) as ExportResult;
    }

    /// <summary>
    /// Posts summary pass count request to Raptor. 
    /// This is a summary of whether the pass count exceeds the target, meets the pass count target, or falls below the target.
    /// </summary>
    /// <param name="request">Summary pass counts request request</param>
    /// <returns>Returns JSON structure wtih operation result.
    /// </returns>
    /// <executor>SummaryPassCountsExecutor</executor>
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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

    /// <summary>
    /// Gets volumes summary from Raptor.
    /// </summary>
    /// <param name="request">The request for volumes summary request to Raptor</param>
    /// <returns></returns>
    /// <executor>SummaryVolumesExecutor</executor>
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
    /// 
    [PostRequestVerifier]
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
