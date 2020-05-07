using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.TRex.Filters;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/sandbox")]
  public class SandboxController : ControllerBase
  {
    [HttpGet("jsonparameter")]
    public JsonResult GetJSONParameter([FromQuery] string param)
    {
      var arg = Encoding.ASCII.GetString(Convert.FromBase64String(param));
      CombinedFilter filter = JsonConvert.DeserializeObject<CombinedFilter>(arg);

      return new JsonResult(filter);
    }
  }
}
