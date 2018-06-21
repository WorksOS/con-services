using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Local.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;

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
    public async Task<FileResult> GetTile(/*[FromBody] TileRequest request*/)
    {

      //TileRequest will contain a FilterResult and other parameters 
      Log.LogDebug("GetTile: " + Request.QueryString);

      //TODO: Validate request parameters
      //TODO set up request and call TRex

      /*
      var tileResult = WithServiceExceptionTryExecuteAsync(async () =>
        RequestExecutorContainer
          .Build<TileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(tileRequest) as TileResult);
         

      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
      */
      throw new NotImplementedException();
    }
  }
}
