#if NET_4_7 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Common.Executors;

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// ponding controller.
  /// </summary>
  public class PondingController : BaseController<PondingController>
  {
    /// <summary>
    /// Generates a ponding pdf from a design file (TIN) using hydro libraries
    /// </summary>
    [HttpPost("api/v1")]
    public async Task<PondingResult> GetPondingImage([FromBody] PondingRequest pondingRequest)
    {
      Log.LogDebug($"{nameof(GetPondingImage)}: request {JsonConvert.SerializeObject(pondingRequest)}");
      pondingRequest.Validate();

      var result = ( await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<PondingExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            //customerUid, userId, // todoJeannie
            null, null,
            null, CustomHeaders, LandLeveling)
          .ProcessAsync(pondingRequest)) as PondingResult
      );

      return result;
    }
  }
}
#endif
