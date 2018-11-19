using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/tiles")]
  public class TileController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Generates a tile...
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="maxY"></param>
    /// <param name="mode"></param>
    /// <param name="pixelsX"></param>
    /// <param name="pixelsY"></param>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="maxX"></param>
    /// <returns></returns>
    [HttpGet("{siteModelID}")]
    public JsonResult GetTile(string siteModelID,
      [FromQuery] double minX,
      [FromQuery] double minY,
      [FromQuery] double maxX,
      [FromQuery] double maxY,
      [FromQuery] int mode,
      [FromQuery] ushort pixelsX,
      [FromQuery] ushort pixelsY)
    {
      var request = new TileRenderRequest();
      TileRenderResponse_Core2 response = request.Execute(new TileRenderRequestArgument(
        siteModelID: Guid.Parse(siteModelID),
        coordsAreGrid: true,
        pixelsX: pixelsX,
        pixelsY: pixelsY,
        extents: new BoundingWorldExtent3D(minX, minY, maxX, maxY),
        mode: (DisplayMode) mode,
        filter1: new CombinedFilter(),
        filter2: new CombinedFilter(),
        referenceDesignId: Guid.Empty
      )) as TileRenderResponse_Core2;

      return new JsonResult(new TileResult(response?.TileBitmapData));
    }

    /// <summary>
    /// Retrieves the list of available tile generation modes
    /// </summary>
    /// <returns></returns>
    [HttpGet("modes")]
    public JsonResult GetModes()
    {
      return new JsonResult(new List<(DisplayMode Index, string Name)>
      {
        (DisplayMode.Height, "Height"),
        (DisplayMode.CCV, "CCV"),
        (DisplayMode.MDP, "MDP"),
        (DisplayMode.CutFill, "Cut/Fill"),
        (DisplayMode.MachineSpeed, "Speed"),
        (DisplayMode.TargetSpeedSummary, "Speed Summary"),
        (DisplayMode.TemperatureSummary, "Temperature Summary")
      });
    }
  }
}
