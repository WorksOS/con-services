using System.Net;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CCSS.WorksOS.Reports.Controllers
{
  public class ReportsController : BaseController<ReportsController>
  {
    [HttpPost("api/v1/reports/stationoffset")]
    public IActionResult GetStationOffsetReport([FromBody] ReportRequest reportRequest)
    {
      reportRequest.ReportTypeEnum = ReportType.StationOffset;
      Log.LogInformation($"{nameof(GetStationOffsetReport)} request: {reportRequest}");
      reportRequest.Validate();

      return StatusCode((int) HttpStatusCode.InternalServerError, $"{nameof(GetStationOffsetReport)} not supported yet");
    }

    [HttpPost("api/v1/reports/summary")]
    public IActionResult GetSummaryReport([FromBody] ReportRequest reportRequest)
    {
      reportRequest.ReportTypeEnum = ReportType.Summary;
      Log.LogInformation($"{nameof(GetSummaryReport)} request: {reportRequest}");
      reportRequest.Validate();

      return StatusCode((int) HttpStatusCode.InternalServerError, $"{nameof(GetSummaryReport)} not supported yet");
    }

    [HttpPost("api/v1/reports/grid")]
    public IActionResult GetGridReport([FromBody] ReportRequest reportRequest)
    {
      reportRequest.ReportTypeEnum = ReportType.Grid;
      Log.LogInformation($"{nameof(GetGridReport)} request: {reportRequest}");
      reportRequest.Validate();

      return StatusCode((int) HttpStatusCode.InternalServerError, $"{nameof(GetGridReport)} not supported yet");
    }
  }
}
