using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
