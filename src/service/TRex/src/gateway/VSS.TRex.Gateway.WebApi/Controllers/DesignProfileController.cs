using System;
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
    /// Get the design profile between a pair of points across a design surface
    /// </summary>
    /// <param name="designProfileRequest"></param>
    /// <returns></returns>
    [Route("api/v1/profile/design")]
    [HttpPost]
    public DesignProfileResult PostDesignProfile([FromBody] DesignProfileRequest designProfileRequest)
    {
      Log.LogInformation($"{nameof(PostDesignProfile)}: {Request.QueryString}");

      designProfileRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DesignProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(designProfileRequest) as DesignProfileResult);
    }

    /// <summary>
    /// Get the design profile between a pair of points across a design surface
    /// </summary>
    /// <param name="designUID"></param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <returns></returns>
    [Route("api/v1/profile/design")]
    [HttpGet]
    public DesignProfileResult GetDesignProfile([FromQuery] Guid designUID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      var designProfileRequest = new DesignProfileRequest(designUID, startX, startY, endX, endY);
      return PostDesignProfile(designProfileRequest);
    }
  }
}
