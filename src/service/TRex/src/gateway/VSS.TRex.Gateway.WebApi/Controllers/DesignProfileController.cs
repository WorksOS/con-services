using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting geometric profile line results across design surfaces.
  /// </summary>
  public class DesignProfileController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public DesignProfileController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DesignProfileController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results.
    /// </summary>
    /// <param name="designProfileRequest"></param>
    /// <returns></returns>
    [Route("api/v1/profile/design")]
    [HttpPost]
    public DesignProfileResult PostSummaryVolumes([FromBody] DesignProfileRequest designProfileRequest)
    {
      Log.LogInformation($"{nameof(PostSummaryVolumes)}: {Request.QueryString}");

      designProfileRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DesignProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(designProfileRequest) as DesignProfileResult);
    }
  }
}
