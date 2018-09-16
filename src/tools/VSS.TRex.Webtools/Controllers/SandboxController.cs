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
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Logging;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/sandbox")]
  public class SandboxController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    [HttpGet("jsonparameter")]
    public JsonResult GetJSONPameter([FromQuery] string param)
    {
      var arg = Encoding.ASCII.GetString(Convert.FromBase64String(param));
      CombinedFilter filter = JsonConvert.DeserializeObject<CombinedFilter>(arg);

      return new JsonResult(filter);
    }
  }
}
