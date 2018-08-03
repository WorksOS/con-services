using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApiModels.Coord.Contracts;
using VSS.Productivity3D.WebApiModels.Coord.Executors;
using VSS.Productivity3D.WebApiModels.Coord.Models;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;

namespace VSS.Productivity3D.WebApi.Coord.Controllers
{
  /// <summary>
  /// Controller for the CoordinateSystemFile resource.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CoordinateSystemController : Controller, ICoordinateSystemFileContract
  {
    private readonly IASNodeClient raptorClient;
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public CoordinateSystemController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    /// <summary>
    /// Posts a coordinate system (CS) definition file to a Raptor's data model/project.
    /// </summary>
    /// <param name="request">The CS definition file structure.</param>
    /// <returns>
    /// Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>CoordinateSystemExecutorPost</executor>
    [PostRequestVerifier]
    [ProjectVerifier(AllowArchivedState = true)]
    [Route("api/v1/coordsystem")]
    [HttpPost]
    public CoordinateSystemSettings Post([FromBody]CoordinateSystemFile request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(logger, raptorClient).Process(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Posts a coordinate system (CS) definition file to a Raptor for validation.
    /// </summary>
    /// <param name="request">The CS definition file structure.</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>CoordinateSystemExecutorPost</executor>
    [PostRequestVerifier]
    [Route("api/v1/coordsystem/validation")]
    [HttpPost]
    public CoordinateSystemSettings PostValidate([FromBody]CoordinateSystemFileValidationRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(logger, raptorClient).Process(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Gets a coordinate system (CS) definition assigned to a Raptor's data model/project.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as coordinate system settings. {"Code":0,"Message":"User-friendly"}
    /// List of codes:
    ///     OK = 0,
    ///     Incorrect Requested Data = -1,
    ///     Validation Error = -2
    ///     InternalProcessingError = -3;
    ///     FailedToGetResults = -4;
    /// </returns>
    /// <executor>CoordinateSystemExecutorGet</executor>
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/coordsystem")]
    [HttpGet]
    public CoordinateSystemSettings Get([FromRoute] long projectId)
    {
      ProjectID request = ProjectID.Create(projectId);

      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(logger, raptorClient).Process(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Gets a coordinate system (CS) definition assigned to a Raptor's data model/project with a unique identifier.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as coordinate system settings. {"Code":0,"Message":"User-friendly"}
    /// List of codes:
    ///     OK = 0,
    ///     Incorrect Requested Data = -1,
    ///     Validation Error = -2
    ///     InternalProcessingError = -3;
    ///     FailedToGetResults = -4;
    /// </returns>
    /// <executor>CoordinateSystemExecutorGet</executor>
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/coordsystem")]
    [HttpGet]
    public async Task<CoordinateSystemSettings> Get([FromRoute] Guid projectUid)
    {
      long projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      ProjectID request = ProjectID.Create(projectId, projectUid);

      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(logger, raptorClient).Process(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Posts a list of coordinates to a Raptor's data model/project for conversion.
    /// </summary>
    /// <param name="request">Description of the coordinate conversion request.</param>
    /// <returns>
    /// Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>CoordinateCoversionExecutor</executor>
    /// 
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/coordinateconversion")]
    [HttpPost]
    public CoordinateConversionResult Post([FromBody]CoordinateConversionRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(logger, raptorClient).Process(request) as CoordinateConversionResult;
    }
  }
}
