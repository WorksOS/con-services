using System;
using Microsoft.AspNetCore.Mvc;

namespace VSS.Productivity3D.Now3D.Controllers
{
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