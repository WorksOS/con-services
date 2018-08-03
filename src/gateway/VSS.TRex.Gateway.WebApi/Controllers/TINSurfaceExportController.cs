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
    public class TINSurfaceExportController : BaseController
    {
//    private ITileRenderingServer tileRenderServer;

    public TINSurfaceExportController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler,
      IConfigurationStore configStore, ITileRenderingServer tileRenderServer)
      : base(loggerFactory, loggerFactory.CreateLogger<TileController>(), exceptionHandler, configStore)
    {
//      this.tileRenderServer = tileRenderServer;
    }

    [HttpPost]
    [Route("api/v1/export/surface/ttm")]
    public FileResult GetTINSurface([FromBody] bool /*ExportTINSurfaceRequest */request)
    {
      Log.LogDebug("GetTINSUrface: " + Request.QueryString);

//      request.Validate();

      //var tileResult = WithServiceExceptionTryExecute(() =>
      //  RequestExecutorContainer
      //    .Build<TileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, tileRenderServer, null)
      //    .Process(request)) as TileResult;

      return null; //new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }
  }
}
