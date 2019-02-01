using System;
using Microsoft.AspNetCore.Mvc;

namespace VSS.Productivity3D.Push
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