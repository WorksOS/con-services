using System.Net;
using System.Threading.Tasks;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using CCSS.WorksOS.Reports.Common.Executors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CCSS.WorksOS.Reports.Controllers
{
  public class ReportsController : BaseController<ReportsController>
  {

    [HttpPost("api/v1/reports/summary")]
    public async Task<IActionResult> GetSummaryReport([FromBody] ReportRequest reportRequest)
    {
      reportRequest.ReportTypeEnum = ReportType.Summary;
      Log.LogInformation($"{nameof(GetSummaryReport)} request: {reportRequest}");
      reportRequest.Validate();

      var reportResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetSummaryExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserUid, customHeaders,
            PreferenceProxy, GracefulClient
          )
          .ProcessAsync(reportRequest));

      return StatusCode((int)HttpStatusCode.InternalServerError, $"{nameof(GetSummaryReport)} not supported yet");
    }

    [HttpPost("api/v1/reports/stationoffset")]
    public async Task<IActionResult> GetStationOffsetReportAsync(
      [FromBody] ReportRequest reportRequest)
    {
      reportRequest.ReportTypeEnum = ReportType.StationOffset;
      Log.LogInformation($"{nameof(GetStationOffsetReportAsync)} request: {reportRequest}");
      reportRequest.Validate();

      var reportResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetStationOffsetExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserUid, customHeaders,
            PreferenceProxy, GracefulClient
            )
          .ProcessAsync(reportRequest));

      return StatusCode((int) HttpStatusCode.InternalServerError, $"{nameof(GetStationOffsetReportAsync)} not supported yet");
    }
    
    [HttpPost("api/v1/reports/grid")]
    public async Task<IActionResult> GetGridReport([FromBody] ReportRequest reportRequest)
    {
      reportRequest.ReportTypeEnum = ReportType.Grid;
      Log.LogInformation($"{nameof(GetGridReport)} request: {reportRequest}");
      reportRequest.Validate();

      var reportResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetGridExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserUid, customHeaders,
            PreferenceProxy, GracefulClient
          )
          .ProcessAsync(reportRequest));

      return StatusCode((int) HttpStatusCode.InternalServerError, $"{nameof(GetGridReport)} not supported yet");
    }
  }
}
