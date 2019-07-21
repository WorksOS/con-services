using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Coords;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting coordinate system definition data from a site model/project
  /// and performing coordinates conversion. 
  /// </summary>
  public class CoordinateSystemController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public CoordinateSystemController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<CoordinateSystemController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Posts a coordinate system (CS) definition file to a TRex's for validation.
    /// </summary>
    [Route("api/v1/coordsystem/validation")]
    [HttpPost]
    public Task<ContractExecutionResult> ValidateCoordinateSystem([FromBody] CoordinateSystemFileValidationRequest request)
    {
      Log.LogInformation($"{nameof(ValidateCoordinateSystem)}: {Request.QueryString}");

      request.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<CoordinateSystemValidationExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(request));
    }

    /// <summary>
    /// Gets a coordinate system (CS) definition assigned to a TRex's site model/project with a unique identifier.
    /// </summary>
    [Route("api/v1/projects/{projectUid}/coordsystem")]
    [HttpGet]
    public Task<ContractExecutionResult> GetCoordinateSystem([FromRoute] Guid projectUid)
    {
      Log.LogInformation($"{nameof(GetCoordinateSystem)}: {Request.QueryString}");

      var request = new ProjectID(null, projectUid);

      request.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<CoordinateSystemGetExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(request));
    }

    /// <summary>
    /// Posts a list of coordinates to a TRex's site model/project for conversion.
    /// </summary>
    [Route("api/v1/coordinateconversion")]
    [HttpPost]
    public Task<ContractExecutionResult> PostCoordinateConversion([FromBody] CoordinateConversionRequest request)
    {
      Log.LogInformation($"{nameof(PostCoordinateConversion)}: {Request.QueryString}");

      request.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<CoordinateConversionExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(request));
    }

    /// <summary>
    /// Gets a coordinate system CSIB assigned to a TRex's site model/project with a unique identifier.
    /// </summary>
    [Route("api/v1/projects/{projectUid}/csib")]
    [HttpGet]
    public CSIBResult GetCSIB([FromRoute] Guid projectUid)
    {
      Log.LogInformation($"{nameof(GetCSIB)}: {Request.QueryString}");

      var request = new ProjectID(null, projectUid);

      request.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CSIBExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(request) as CSIBResult);
    }

  }
}
