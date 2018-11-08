using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/switchablegrid")]
  public class SwitchableGridController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Switches to mutable grid
    /// </summary>
    /// <returns></returns>
    [HttpPut("mutable")]
    public JsonResult SwitchToMutable()
    {
      SwitchableGridContext.switchableMutability = StorageMutability.Mutable;
      return new JsonResult("OK");
    }

    /// <summary>
    /// Switches to mutable grid
    /// </summary>
    /// <returns></returns>
    [HttpPut("immutable")]
    public JsonResult SwitchToImmutable()
    {
      SwitchableGridContext.switchableMutability = StorageMutability.Immutable;
      return new JsonResult("OK");
    }
  }
}
