using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/profiles")]
  public class ProfilesController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProfilesController>();

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="designID"></param>
    /// <returns></returns>
    [HttpGet("design/{siteModelID}/{designID}")]
    public JsonResult DeleteDesignFromSiteModel(string siteModelID, string designID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      // This is a direct implementation rather than delegating the request to an application service context
      // through a grid request
      Guid siteModelUid = Guid.Parse(siteModelID);
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelUid);
      var design = siteModel?.Designs?.Locate(Guid.Parse(designID));

      if (design == null)
        return new JsonResult($"Unable to locate design {designID} in project {siteModelID}");

      var result = design.ComputeProfile(siteModelUid, new[] {new XYZ(startX, startY, 0), new XYZ(endX, endY, 0)}, siteModel.Grid.CellSize, out DesignProfilerRequestResult errCode);

      return new JsonResult(result);
    }
  }
}
