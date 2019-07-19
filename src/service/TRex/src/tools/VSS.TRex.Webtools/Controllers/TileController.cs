using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
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
    /// Generates a tile.
    /// </summary>
    [HttpPost("{siteModelID}")]
    public async Task<JsonResult> GetTile(
      [FromRoute]string siteModelID,
      [FromQuery] double minX,
      [FromQuery] double minY,
      [FromQuery] double maxX,
      [FromQuery] double maxY,
      [FromQuery] int mode,
      [FromQuery] ushort pixelsX,
      [FromQuery] ushort pixelsY,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] double? offset,
      [FromBody] OverrideParameters overrides)
    {
      var request = new TileRenderRequest();

      //TODO: use overrides to construct color palette
      //(see TileExecutor in TRex Gateway)

      var response = await request.ExecuteAsync(new TileRenderRequestArgument(
        Guid.Parse(siteModelID),
        (DisplayMode)mode,
        null,
        new BoundingWorldExtent3D(minX, minY, maxX, maxY),
        true,
        pixelsX,
        pixelsY,
        new FilterSet(new CombinedFilter(), new CombinedFilter()),
        new DesignOffset(cutFillDesignUid ?? Guid.Empty, offset ?? 0.0)
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
