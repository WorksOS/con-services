#if NET_4_7
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Http;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Common.Executors;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

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
    [Route("internal/api/v1")]
    [Route("api/v1")]
    [HttpPost]
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

    /// <summary>
    /// Create a scheduler job to a) get ttm from 3dp and b) process ttm into images 
    /// </summary>
    /// <returns>Scheduler Job Result, containing the Job ID To poll via the Scheduler</returns>
    [Route("api/v1/background")]
    [HttpPost]
    public async Task<ScheduleJobResult> RequestGetHydroImagesBackgroundJob([FromBody] HydroRequest hydroRequest, [FromServices] ISchedulerProxy scheduler)
    {
      Log.LogDebug($"{nameof(RequestGetHydroImagesBackgroundJob)}: request {JsonConvert.SerializeObject(hydroRequest)}");
      if (hydroRequest == null)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 25);

      var baseUrl = ConfigStore.GetValueString("HYDRO_INTERNAL_BASE_URL");
      var callbackUrl = $"{baseUrl}/internal/api/v1";

      var request = new ScheduleJobRequest
      {
        Filename = hydroRequest.FileName + Guid.NewGuid(), // Make sure the filename is unique, it's not important what it's called as the scheduled job keeps a reference
        Method = "POST",
        Url = callbackUrl,
        Headers =
        {
          ["Content-Type"] = Request.Headers["Content-Type"]
        }
      };
      request.SetBinaryPayload(Request.Body);

      var customHeaders = Request.Headers.GetCustomHeaders();

      return await scheduler.ScheduleBackgroundJob(request, customHeaders);
    }
  }
}
#endif
