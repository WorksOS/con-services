using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Rendering.Servers.Client;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  public class TileController : BaseController
  {
    private ITileRenderingServer tileRenderServer;

    public TileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, 
      IConfigurationStore configStore, ITileRenderingServer tileRenderServer)
      : base(loggerFactory, loggerFactory.CreateLogger<TileController>(), exceptionHandler, configStore)
    {
      this.tileRenderServer = tileRenderServer;
    }

    [HttpPost]
    [Route("api/v1/tile")]
    public FileResult GetTile([FromBody] TileRequest request)
    {
      //TileRequest will contain a FilterResult and other parameters 
      Log.LogDebug("GetTile: " + Request.QueryString);
      
      request.Validate();

      var tileResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<TileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, tileRenderServer)
          .Process(request)) as TileResult;

      if (tileResult?.TileData == null )
        tileResult = TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);

      var memStream = new MemoryStream();
      tileResult.TileData.Save(memStream, ImageFormat.Png);
      memStream.Position = 0;

      return new FileStreamResult(memStream, "image/png");
    }
  }
}
