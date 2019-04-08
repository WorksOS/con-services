using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data image tiles.
  /// </summary>
  [Route("api/v1/tile")]
  public class TileController : BaseController
  {
    /// <summary>
    /// Constructor for production data image tile controller.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    public TileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, 
      IConfigurationStore configStore): base(loggerFactory, loggerFactory.CreateLogger<TileController>(), exceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Gets production data image tile.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public FileResult GetTile([FromBody] TileRequest request)
    {
      Log.LogInformation($"{nameof(GetTile)}: {Request.QueryString}");

      request.Validate();
      if (request.ProjectUid == null || request.ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      var siteModel = GatewayHelper.ValidateAndGetSiteModel(request.ProjectUid.Value, nameof(GetTile));
      if (request.Filter1 != null && request.Filter1.ContributingMachines != null)
        GatewayHelper.ValidateMachines(
          request.Filter1.ContributingMachines.Select(m => m.AssetUid).ToList(), siteModel);


      var tileResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<TileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(request)) as TileResult;


      if (tileResult?.TileData == null)
        tileResult = TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);

      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }
  }
}
