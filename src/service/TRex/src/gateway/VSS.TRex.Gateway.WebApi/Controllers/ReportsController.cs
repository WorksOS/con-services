using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting report data.
  /// </summary>
  public class ReportsController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public ReportsController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DetailsDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get station-offset report stream for the specified project, filter etc.
    /// </summary>
    /// <param name="stationOffsetRequest"></param>
    /// <returns></returns>
    [Route("api/v1/report/stationoffset")]
    [HttpPost]
    public FileResult PostStationOffsetReport([FromBody] CompactionReportStationOffsetRequest stationOffsetRequest)
    {
      Log.LogInformation($"{nameof(PostStationOffsetReport)}: {Request.QueryString}");

      stationOffsetRequest.Validate();

      throw new ServiceException(HttpStatusCode.NotImplemented, 
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"StationOffset report has not been implemented in Trex yet. ProjectUid: {stationOffsetRequest.ProjectUid}"));
    }

    /// <summary>
    /// Get grid report for the specified project, filter etc.
    /// </summary>
    /// <param name="reportGridRequest"></param>
    /// <returns></returns>
    [Route("api/v1/report/grid")]
    [HttpPost]
    public FileResult PostGriddedReport([FromBody] CompactionReportGridRequest reportGridRequest)
    {
      Log.LogInformation($"{nameof(PostGriddedReport)}: {Request.QueryString}");

      reportGridRequest.Validate();

      var griddedReportDataResult =  WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<GriddedReportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(reportGridRequest) as GriddedReportDataResult);

      if (griddedReportDataResult?.GriddedData == null)
      {
        var code = griddedReportDataResult == null ? HttpStatusCode.BadRequest : HttpStatusCode.NoContent;
        var exCode = griddedReportDataResult == null ? ContractExecutionStatesEnum.FailedToGetResults : ContractExecutionStatesEnum.ValidationError;

        throw new ServiceException(code, new ContractExecutionResult(exCode, $"Failed to get gridded report data for projectUid: {reportGridRequest.ProjectUid}"));
      }

      return new FileStreamResult(new MemoryStream(griddedReportDataResult?.GriddedData), "application/octet-stream");
    }
  }
}
