using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
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
    public IActionResult GetTile([FromBody] QMTileRequest request)
    {

      Log.LogInformation($"{nameof(GetTile)}: #Tile# In. Params. XYZ:{request.X},{request.Y},{request.Z}, Project:{request.ProjectUid}, Lighting:{request.HasLighting}");

      request.Validate();

      try
      {
        var tileResult = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<QuantizedMeshTileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(request)) as QMTileResult;
        if (tileResult == null || tileResult.TileData == null)
        {
          var msg = $"Failed to get Quantized Mesh tile for projectUid: {request.ProjectUid}. Check log VSS.TRex.Server.QuantizedMesh";
          Log.LogError(msg);
          return NoContent();
        }

        Log.LogDebug($"#Tile# Out. XYZ:{request.X},{request.Y},{request.Z}");
        return new FileStreamResult(new MemoryStream(tileResult.TileData), ContentTypeConstants.ApplicationOctetStream);
      }
      catch (System.Exception e)
      {
        // log exception in Gateway log then return exception. Typically cluster not active
        var msg = $"Failed to execute Quantized Mesh tile generation for projectUid: {request.ProjectUid} Error:{e.Message}";
        Log.LogError(msg);
        throw;
      }
    }
  }
}
