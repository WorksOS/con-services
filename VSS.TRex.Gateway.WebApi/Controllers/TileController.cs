using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Local.ResultHandling;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Models;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  public class TileController : BaseController
  {
    public TileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<TileController>(), exceptionHandler, configStore)
    {     
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
          .Build<TileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(request)) as TileResult;

      if (tileResult?.TileData == null )
        tileResult = TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);

      var memStream = new MemoryStream();
      ((System.Drawing.Bitmap) tileResult.TileData.GetBitmap()).Save(memStream, ImageFormat.Png);
      memStream.Position = 0;

      return new FileStreamResult(memStream, "image/png");
    }
  }
}
