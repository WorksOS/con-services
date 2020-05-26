using System.IO;
using System.Threading.Tasks;
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
    /// Constructor for production data quantized mesh terrain tile controller.
    /// </summary>
    public TerrainController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler,
      IConfigurationStore configStore) : base(loggerFactory, loggerFactory.CreateLogger<TerrainController>(), exceptionHandler, configStore)
    {
    }

    [HttpGet]
    public string Get()
    {
      return "You have reached the Gateway Terrain Controller";
    }

    /// <summary>
    /// Get production quantized mesh tile.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetTile([FromBody] QMTileRequest request)
    {
      Log.LogInformation($"{nameof(GetTile)}: #Tile# In. Params. XYZ:{request.X},{request.Y},{request.Z}, Project:{request.ProjectUid}, Lighting:{request.HasLighting}, DisplayMode:{request.DisplayMode}");
      request.Validate();

      var tileResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<QuantizedMeshTileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(request)) as QMTileResult;

      if (tileResult == null || tileResult.TileData == null)
      {
        Log.LogError($"Failed to get Quantized Mesh tile for projectUid: {request.ProjectUid}. For more info check .Server.QuantizedMesh.log");
        return NoContent();
      }

      Log.LogDebug($"#Tile# Out. XYZ:{request.X},{request.Y},{request.Z}");
      return new FileStreamResult(new MemoryStream(tileResult.TileData), ContentTypeConstants.ApplicationOctetStream);
    }

  }
}
