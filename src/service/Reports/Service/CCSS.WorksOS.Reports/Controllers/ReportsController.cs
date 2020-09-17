using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CCSS.WorksOS.Reports.Controllers
{
  public class ReportsController : BaseController<ReportsController>
  {
    [HttpGet("api/v1/reports/stationoffset")]
    public IActionResult GetStationOffsetReport()
    {
      Log.LogInformation($"{nameof(GetStationOffsetReport)} Hit the endpoint");

      return StatusCode((int) HttpStatusCode.InternalServerError, $"{nameof(GetStationOffsetReport)} not supported yet");
    }

    [HttpGet("api/v1/reports/summary")]
    public IActionResult GetSummaryReport()
    {
      Log.LogInformation($"{nameof(GetSummaryReport)} Hit the endpoint");

      return StatusCode((int)HttpStatusCode.InternalServerError, $"{nameof(GetSummaryReport)} not supported yet");
    }

    [HttpGet("api/v1/reports/grid")]
    public IActionResult GetGridReport()
    {
      Log.LogInformation($"{nameof(GetGridReport)} Hit the endpoint");

      return StatusCode((int)HttpStatusCode.InternalServerError, $"{nameof(GetGridReport)} not supported yet");
    }
  }
}
