using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/designs")]
  public class DesignController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DesignController>();

    /// <summary>
    /// Returns the list of designs registered for a sitemodel. If there are no designs the
    /// result will be an empty list.
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}")]
    public JsonResult GetSurveyedSurfacesForSiteModel(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<IDesignManager>().List(Guid.Parse(siteModelID)));
    }

    /// <summary>
    /// Deletes a design from a sitemodel.
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="designID"></param>
    /// <returns></returns>
    [HttpDelete("{siteModelID}/{designID}")]
    public JsonResult DeleteSurveyedSurfacesFromSiteModel(string siteModelID, string designID)
    {
      return new JsonResult(DIContext.Obtain<IDesignManager>().Remove(Guid.Parse(siteModelID), Guid.Parse(designID)));
    }

    /// <summary>
    /// Adds a new design to a sitemodel. 
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="fileName"></param>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <returns></returns>
    [HttpPost("{siteModelID}")]
    public JsonResult AddSurveyedSurfacesToSiteModel(string siteModelID,
      [FromQuery] string fileName,
      [FromQuery] double minX,
      [FromQuery] double minY,
      [FromQuery] double maxX,
      [FromQuery] double maxY)
    {
      return new JsonResult(DIContext.Obtain<IDesignManager>().Add
        (Guid.Parse(siteModelID), 
        new DesignDescriptor(Guid.NewGuid(), "", "", "", fileName, 0),  
        new BoundingWorldExtent3D(minX, minY, maxX, maxY)));
    }
  }
}
