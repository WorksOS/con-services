using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

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
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <returns></returns>
    [HttpGet("design/{siteModelID}/{designID}")]
    public JsonResult ComputeDesignProfile(string siteModelID, string designID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      Guid siteModelUid = Guid.Parse(siteModelID);
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelUid);
      var design = siteModel?.Designs?.Locate(Guid.Parse(designID));

      if (design == null)
        return new JsonResult($"Unable to locate design {designID} in project {siteModelID}");

      var result = design.ComputeProfile(siteModelUid, new[] {new XYZ(startX, startY, 0), new XYZ(endX, endY, 0)}, siteModel.Grid.CellSize, out DesignProfilerRequestResult errCode);

      return new JsonResult(result);
    }

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <returns></returns>
    [HttpGet("productiondata/{siteModelID}")]
    public JsonResult ComputeProductionDataProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      Guid siteModelUid = Guid.Parse(siteModelID);
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelUid);

      ProfileRequestArgument_ApplicationService arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        PositionsAreGrid = true,
        Filters = new FilterSet(new[] { new CombinedFilter() }),
        ReferenceDesignID = Guid.Empty,
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
        DesignDescriptor = DesignDescriptor.Null()
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      ProfileRequest_ApplicationService request = new ProfileRequest_ApplicationService();
      ProfileRequestResponse Response = request.Execute(arg);

      if (Response == null)
        return new JsonResult(@"Profile response is null");
      
      if (Response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      var nonNulls = Response.ProfileCells.Where(x => ((ProfileCell) x).CellLastElev != Consts.NullHeight).ToArray();
      return new JsonResult(Response.ProfileCells.Select(x => new XYZS(0, 0, x.Station, ((ProfileCell)x).CellLastElev, -1) ));
    }
  }
}
