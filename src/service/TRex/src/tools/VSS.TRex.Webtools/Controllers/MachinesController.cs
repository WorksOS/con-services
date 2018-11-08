using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/machines")]
  public class MachinesController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<MachinesController>();

    /// <summary>
    /// Returns the list of machines for a sitemodel. If there are no machine the
    /// result will be an empty list.
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}")]
    public JsonResult GetMachinesSiteModel(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.Machines);
    }

    /// <summary>
    /// Returns a single machine for a sitemodel
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="machineID"></param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/{machineID}")]
    public JsonResult GetMachineForSiteModel(string siteModelID, string machineID)
    {
      return new JsonResult(DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.Machines.Locate(Guid.Parse(machineID)));
    }
  }
}
