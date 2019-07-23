using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  [Route("api/v1/terrain")]
  [ApiController]
  public class TerrainController : BaseController
  {
    /// <summary>
    /// Constructor for production data image tile controller.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    public TerrainController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler,
      IConfigurationStore configStore) : base(loggerFactory, loggerFactory.CreateLogger<TerrainController>(), exceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get production quantized mesh tile.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public FileResult GetTile([FromBody] QMTileRequest request)
    {
      Log.LogInformation($"{nameof(GetTile)}: {Request.QueryString}");

      request.Validate();

      var tileResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<QuantizedMeshTileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(request)) as QMTileResult;

      return new FileStreamResult(new MemoryStream(tileResult.TileData), ContentTypeConstants.ApplicationOctetStream);
    }


  }
}
