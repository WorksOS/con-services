using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.Models;
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
    /// <param name="offset"></param>
    /// <returns></returns>
    [HttpGet("design/{siteModelID}/{designID}")]
    public async Task<JsonResult> ComputeDesignProfile(string siteModelID, string designID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY,
      [FromQuery] double? offset)
    {
      var siteModelUid = Guid.Parse(siteModelID);
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelUid);
      var design = siteModel?.Designs?.Locate(Guid.Parse(designID));

      if (design == null)
        return new JsonResult($"Unable to locate design {designID} in project {siteModelID}");

      var result = await design.ComputeProfile(siteModelUid, new[] { new XYZ(startX, startY, 0), new XYZ(endX, endY, 0) }, siteModel.CellSize, offset ?? 0);

      return new JsonResult(result.profile);
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
    [HttpGet("compositeelevations/{siteModelID}")]
    public JsonResult ComputeCompositeElevationProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      Guid siteModelUid = Guid.Parse(siteModelID);

      ProfileRequestArgument_ApplicationService arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new[] { new CombinedFilter() }),
        ReferenceDesign = new DesignOffset(),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_ProfileCell();
      var Response = request.Execute(arg);

      if (Response == null)
        return new JsonResult(@"Profile response is null");

      if (Response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      //var nonNulls = Response.ProfileCells.Where(x => !x.IsNull()).ToArray();
      return new JsonResult(Response.ProfileCells.Select(x => new
      {
        station = x.Station,
        cellLowestElev = x.CellLowestElev,
        cellHighestElev = x.CellHighestElev,
        cellLastElev = x.CellLastElev,
        cellFirstElev = x.CellFirstElev,
        cellLowestCompositeElev = x.CellLowestCompositeElev,
        cellHighestCompositeElev = x.CellHighestCompositeElev,
        cellLastCompositeElev = x.CellLastCompositeElev,
        cellFirstCompositeElev = x.CellFirstCompositeElev
      }));
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

      ProfileRequestArgument_ApplicationService arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new[] { new CombinedFilter() }),
        ReferenceDesign = new DesignOffset(),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_ProfileCell();
      var Response = request.Execute(arg);

      if (Response == null)
        return new JsonResult(@"Profile response is null");

      if (Response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      //var nonNulls = Response.ProfileCells.Where(x => !x.IsNull()).ToArray();
      return new JsonResult(Response.ProfileCells.Select(x => new XYZS(0, 0, x.CellLastElev, x.Station, -1)));
    }


    [HttpGet("volumes/{siteModelID}")]
    public JsonResult ComputeSummaryVolumesProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      Guid siteModelUid = Guid.Parse(siteModelID);

      ProfileRequestArgument_ApplicationService arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.SummaryVolume,
        PositionsAreGrid = true,
        Filters = new FilterSet(new CombinedFilter(), new CombinedFilter()),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
        VolumeType = VolumeComputationType.Between2Filters
      };

      // This is a simple earliest filter to latest filter test
      arg.Filters.Filters[0].AttributeFilter.ReturnEarliestFilteredCellPass = true;
      arg.Filters.Filters[1].AttributeFilter.ReturnEarliestFilteredCellPass = false;

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_SummaryVolumeProfileCell();

      var Response = request.Execute(arg);
      if (Response == null)
        return new JsonResult(@"Profile response is null");

      if (Response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      return new JsonResult(Response.ProfileCells.Select(x => new XYZS(0, 0, x.LastCellPassElevation2 - x.LastCellPassElevation1, x.Station, -1)));
    }
  }
}
