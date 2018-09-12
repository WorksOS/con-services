using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Logging;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

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
      public JsonResult GridStatus(string siteModelID, 
        [FromQuery] int minX,
        [FromQuery] int minY,
        [FromQuery] int maxX,
        [FromQuery] int maxY,
        [FromQuery] int mode,
        [FromQuery] ushort pixelsX,
        [FromQuery] ushort pixelsY)
      {
        ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID));

        var request = new TileRenderRequest();
        var response = request.Execute(new TileRenderRequestArgument(
          siteModelID: Guid.Parse(siteModelID),
          coordsAreGrid: true,
          pixelsX: pixelsX,
          pixelsY: pixelsY,
          extents: new BoundingWorldExtent3D(minX, minY, maxX, maxY),
          mode: (DisplayMode) mode,
          filter1: null,
          filter2: null,
          cutFillDesignID: Guid.Empty
        ));

        return new JsonResult(TileResult.CreateTileResult(response.));
      }
  }
}
