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
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// retrieves images for original ground.
  /// </summary>
  public class HydroController : BaseController<HydroController>
  {
    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    private IRaptorProxy raptorProxy;

    private IRaptorProxy RaptorProxy => raptorProxy ?? (raptorProxy = HttpContext.RequestServices.GetService<IRaptorProxy>());

    /// <summary>
    /// Generates a zip containing hydrology images from the original ground from a design file (TIN).
    /// The images can include e.g. ponding and drainage pdfs, which are created using the hydro libraries
    /// </summary>
    [HttpPost("api/v1")]
    public async Task<FileResult> GetHydroImages([FromBody] HydroRequest hydroRequest)
    {
      Log.LogDebug($"{nameof(GetHydroImages)}: request {JsonConvert.SerializeObject(hydroRequest)}");
      hydroRequest.Validate();

      var result = ( await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<HydroExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            null, null,
            null, CustomHeaders, LandLeveling, RaptorProxy)
          .ProcessAsync(hydroRequest)) as HydroResult
      );

      var fileStream = new FileStream(result.FullFileName, FileMode.Open);

      Log.LogInformation($"{nameof(GetHydroImages)} completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, ContentTypeConstants.ApplicationZip);
    }
  }
}
#endif
