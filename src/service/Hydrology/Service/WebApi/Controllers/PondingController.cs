using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Common.Executors;
using VSS.MasterData.Models.Handlers;

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// ponding controller.
  /// </summary>
  public class PondingController : BaseController<PondingController>
  {
    /// <inheritdoc />
    public PondingController(ILoggerFactory loggerFactory, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler) :
      base(configStore) { }

#if NET_4_7 
    /// <summary>
    /// Generates a ponding pdf from a design file (TIN) using hydro libraries
    /// </summary>
    [HttpPost("api/v1/ponding")]
    public async Task<PondingResult> GetPondingImage(PondingRequest pondingRequest)
    {
      // todo find a way to copy hydro sublibraries AND .tx files
      // these must be manually copied before starting testing
      Log.LogDebug($"{nameof(GetPondingImage)}: request {JsonConvert.SerializeObject(pondingRequest)}");
      pondingRequest.Validate();

      var result = ( await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<PondingExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, CustomHeaders)
          .ProcessAsync(pondingRequest))  as PondingResult
      );

      return result;
    }
#endif
  }
}
