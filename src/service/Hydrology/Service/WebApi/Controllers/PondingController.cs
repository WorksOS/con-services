#if NET_4_7
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Http;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Common.Executors;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// ponding controller.
  /// </summary>
  public class PondingController : BaseController<PondingController>
  {
    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    private IRaptorProxy _raptorProxy;
    protected IRaptorProxy RaptorProxy => _raptorProxy ?? (_raptorProxy = HttpContext.RequestServices.GetService<IRaptorProxy>());

    /// <summary>
    /// Generates a ponding pdf from a design file (TIN) using hydro libraries
    /// </summary>
    [HttpPost("api/v1")]
    public async Task<FileResult> GetPondingImage([FromBody] PondingRequest pondingRequest)
    {
      Log.LogDebug($"{nameof(GetPondingImage)}: request {JsonConvert.SerializeObject(pondingRequest)}");
      pondingRequest.Validate();

      var result = ( await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<PondingExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            //customerUid, userId, // todoJeannie
            null, null,
            null, CustomHeaders, LandLeveling, RaptorProxy)
          .ProcessAsync(pondingRequest)) as PondingResult
      );

      var fileStream = new FileStream(result.FullFileName, FileMode.Open);

      Log.LogInformation($"{nameof(GetPondingImage)} completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, ContentTypeConstants.ApplicationZip);
    }
  }
}
#endif
