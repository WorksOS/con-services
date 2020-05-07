using System;
using Microsoft.AspNetCore.Mvc;

namespace VSS.Productivity3D.TagFileGateway.Controllers
{
  // Hide from swagger
  [ApiExplorerSettings(IgnoreApi=true)]
  public class HeartbeatController : Controller
  {
    [HttpGet("/")]
    public IActionResult Echo()
    {
      return Json(new
      {
        CurrentTime = DateTime.UtcNow,
      });
    }
  }
}
