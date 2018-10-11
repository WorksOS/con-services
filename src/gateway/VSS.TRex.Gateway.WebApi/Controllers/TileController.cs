using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data image tiles.
  /// </summary>
  [Route("api/v1/tile")]
  public class TileController : BaseController
  {
    public TileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, 
      IConfigurationStore configStore): base(loggerFactory, loggerFactory.CreateLogger<TileController>(), exceptionHandler, configStore)
    {
    }

    [HttpPost]
    public TileResult GetTile([FromBody] TileRequest request)
    {
      Log.LogInformation($"{nameof(GetTile)}: {Request.QueryString}");

      return GetTileResult(request);
    }

    [HttpPost("filestream")]
    public FileResult GetTileFileStream([FromBody] TileRequest request)
    {
      Log.LogInformation($"{nameof(GetTileFileStream)}: {Request.QueryString}");

      var tileResult = GetTileResult(request);

      if (tileResult?.TileData == null)
        tileResult = TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);

      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }

    private TileResult GetTileResult(TileRequest request)
    {
      request.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<TileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(request)) as TileResult;
    }
  }
}
