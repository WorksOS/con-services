using System;
using Microsoft.AspNetCore.Mvc;

namespace CCSS.WorksOS.Healthz.Controllers
{
  // Hide from swagger
  [ApiExplorerSettings(IgnoreApi = true)]
  public class HeartbeatController : BaseController<HeartbeatController>
  {
    [HttpGet("/")]
    public IActionResult Echo() => Ok($"{DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString()}");
  }
}
