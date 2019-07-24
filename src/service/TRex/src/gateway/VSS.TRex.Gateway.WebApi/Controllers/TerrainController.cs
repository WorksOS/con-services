using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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

      if (tileResult?.TileData == null)
      {
        var code = tileResult == null ? HttpStatusCode.BadRequest : HttpStatusCode.NoContent;
        var exCode = tileResult == null ? ContractExecutionStatesEnum.FailedToGetResults : ContractExecutionStatesEnum.ValidationError;
        throw new ServiceException(code, new ContractExecutionResult(exCode, $"Failed to get Quantized Mesh tile for projectUid: {request.ProjectUid}"));
      }

      return new FileStreamResult(new MemoryStream(tileResult.TileData), ContentTypeConstants.ApplicationOctetStream);
    }


  }
}
