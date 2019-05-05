using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/tiles")]
  public class TileController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Generates a tile.
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
    public async Task<JsonResult> GetTile(string siteModelID,
      [FromQuery] double minX,
      [FromQuery] double minY,
      [FromQuery] double maxX,
      [FromQuery] double maxY,
      [FromQuery] int mode,
      [FromQuery] ushort pixelsX,
      [FromQuery] ushort pixelsY)
    {
      var request = new TileRenderRequest();
      var response = await request.ExecuteAsync(new TileRenderRequestArgument(
        siteModelID: Guid.Parse(siteModelID), 
        palette: null,
        coordsAreGrid: true,
        pixelsX: pixelsX,
        pixelsY: pixelsY,
        extents: new BoundingWorldExtent3D(minX, minY, maxX, maxY),
        mode: (DisplayMode) mode,
        filters: new FilterSet(new CombinedFilter(), new CombinedFilter()),
        referenceDesignUid: Guid.Empty
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
        (DisplayMode.CCVPercentSummary, "CCV Summary"),
        (DisplayMode.PassCount, "Pass Count"),
        (DisplayMode.PassCountSummary, "Pass Count Summary"),
        (DisplayMode.MDPPercentSummary, "MDP Summary"),
        (DisplayMode.CutFill, "Cut/Fill"),
        (DisplayMode.MachineSpeed, "Speed"),
        (DisplayMode.TargetSpeedSummary, "Speed Summary"),
        (DisplayMode.TemperatureSummary, "Temperature Summary"),
        (DisplayMode.CCA, "CCA"),
        (DisplayMode.CCASummary, "CCA Summary")
      });
    }
  }
}
