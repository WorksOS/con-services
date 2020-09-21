using Microsoft.AspNetCore.Mvc;

namespace VSS.WebApi.Common
{
  [ApiController]
  public class PingController : Controller
  {
    [HttpGet("/ping")]
    public IActionResult Ping()
    {
      return Ok("pong");
    }
  }
}
