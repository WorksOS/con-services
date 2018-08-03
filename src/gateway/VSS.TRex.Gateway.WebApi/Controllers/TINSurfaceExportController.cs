using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.Servers.Client;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// The controller for generating TIN surfaces decimatied from elevation data
  /// </summary>
  public class TINSurfaceExportController : BaseController
  {
    private ITINSurfaceExportRequestServer tinSurfaceExportServer;

    /// <summary>
    /// Constructor for TIN surface export controller
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    /// <param name="tinSurfaceExportServer"></param>
    public TINSurfaceExportController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler,
      IConfigurationStore configStore, ITINSurfaceExportRequestServer tinSurfaceExportServer)
      : base(loggerFactory, loggerFactory.CreateLogger<TileController>(), exceptionHandler, configStore)
    {
      this.tinSurfaceExportServer = tinSurfaceExportServer;
    }

    /// <summary>
    /// Web service end point controller for TIN surface export
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/v1/export/surface/ttm")]
    public TINSurfaceExportResult GetTINSurface([FromBody] TileRequest request)
    {
      Log.LogDebug("GetTINSurface: " + Request.QueryString);

      request.Validate();

      var container = RequestExecutorContainer.Build<TINSurfaceExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, null);
      container.tINSurfaceExportRequestServer = tinSurfaceExportServer;

      var tinResult = WithServiceExceptionTryExecute(() => container.Process(request)) as TINSurfaceExportResult;

      return tinResult;
    }
  }
}
