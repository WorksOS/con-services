using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Design;

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
    /// Get the design profile between a pair of points across a design surface
    /// </summary>
    /// <param name="designProfileRequest"></param>
    /// <returns></returns>
    [Route("api/v1/profile/design")]
    [HttpPost]
    public Task<ContractExecutionResult> PostDesignProfile([FromBody] DesignProfileRequest designProfileRequest)
    {
      Log.LogInformation($"{nameof(PostDesignProfile)}: {Request.QueryString}");

      designProfileRequest.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<DesignProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(designProfileRequest));
    }

    /// <summary>
    /// Get the design profile between a pair of points across a design surface
    /// </summary>
    /// <param name="projectUID"></param>
    /// <param name="designUID"></param>
    /// <param name="offset"></param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <returns></returns>
    [Route("api/v1/profile/design")]
    [HttpGet]
    public Task<ContractExecutionResult> GetDesignProfile(
      [FromQuery] Guid projectUID,
      [FromQuery] Guid designUID,
      [FromQuery] double offset,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      return PostDesignProfile(new DesignProfileRequest(projectUID, designUID, offset, startX, startY, endX, endY));
    }
  }
}
