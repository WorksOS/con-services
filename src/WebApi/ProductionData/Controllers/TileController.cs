using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TileController : Controller, ITileContract
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public TileController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    /// <summary>
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
    /// elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="request">A representation of the tile rendering request.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor>
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/tiles")]
    [HttpPost]
    public TileResult Post([FromBody] TileRequest request)
    {
      request.Validate();
      var tileResult = RequestExecutorContainerFactory.Build<TilesExecutor>(logger, raptorClient).Process(request) as TileResult;
      return tileResult;
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="request">A representation of the tile rendering request.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).</returns>
    /// <executor>TilesExecutor</executor>
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/tiles/png")]
    [HttpPost]
    public FileResult PostRaw([FromBody] TileRequest request)
    {
      request.Validate();
      if (RequestExecutorContainerFactory.Build<TilesExecutor>(logger, raptorClient).Process(request) is TileResult tileResult)
      {
        Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

        return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
      }

      return null;
    }
  }
}
