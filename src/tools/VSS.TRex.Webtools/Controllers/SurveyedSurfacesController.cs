using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/surveyedSurfaces")]
  public class SurveyedSurfaceController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SurveyedSurfaceController>();

    /// <summary>
    /// Returns the list of surveyed surfaces registered for a sitemodel. If there are no surveyed surfaces the
    /// result will be an empty list.
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}")]
    public JsonResult GetSurveyedSurfacesForSiteModel(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<ISurveyedSurfaceManager>().List(Guid.Parse(siteModelID)));
    }

    /// <summary>
    /// Deletes a surveyed surfaces from a sitemodel.
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="surveyedSurfaceID"></param>
    /// <returns></returns>
    [HttpDelete("{siteModelID}/{surveyedSurfaceID}")]
    public JsonResult DeleteSurveyedSurfacesFromSiteModel(string siteModelID, string surveyedSurfaceID)
    {
      return new JsonResult(DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID)).SurveyedSurfaces.);
    }

    /// <summary>
    /// Adds a new surveyed surfaces to a sitemodel. 
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="fileName"></param>
    /// <param name="offset"></param>
    /// <param name="asAtDate"></param>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <returns></returns>
    [HttpPost("{siteModelID}")]
    public JsonResult AddSurveyedSurfacesToSiteModel(string siteModelID,
      [FromQuery] string fileName,
      [FromQuery] double offset,
      [FromQuery] string asAtDate,
      [FromQuery] double minX,
      [FromQuery] double minY,
      [FromQuery] double maxX,
      [FromQuery] double maxY)

      //[FromQuery] string newsurveyedsurface)
    {
      // Use a simple anonymouse type to pull the required fields from the supplied json
      //      var ss = new
      //      {
      //        descriptor = default(VSS.TRex.Designs.Models.DesignDescriptor),
      //        asAtDate = DateTime.MinValue,
      //        extents = BoundingWorldExtent3D.Null()
      //      };

      //      var paramAString = Encoding.ASCII.GetString(Convert.FromBase64String(newsurveyedsurface));
      //      JsonConvert.DeserializeAnonymousType(paramAString, ss);

      //      return new JsonResult(DIContext.Obtain<ISurveyedSurfaceManager>().Add(Guid.Parse(siteModelID), ss.descriptor, ss.asAtDate, ss.extents));

      return new JsonResult(DIContext.Obtain<ISurveyedSurfaceManager>().Add
        (Guid.Parse(siteModelID), 
        new DesignDescriptor(Guid.NewGuid(), "", "", "", fileName, offset),  
        DateTime.Parse(asAtDate), 
        new BoundingWorldExtent3D(minX, minY, maxX, maxY)));
    }
  }
}
