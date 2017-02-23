using System;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.WebApiModels.Coord.Contracts;
using VSS.Raptor.Service.WebApiModels.Coord.Executors;
using VSS.Raptor.Service.WebApiModels.Coord.Models;
using VSS.Raptor.Service.WebApiModels.Coord.ResultHandling;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApi.Coord.Controllers
{
  /// <summary>
  /// Controller for the CoordinateSystemFile resource.
  /// </summary>
  /// 
  public class CoordinateSystemController : Controller, ICoordinateSystemFileContract
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Used to get list of projects for customer
    /// </summary>
    private readonly IAuthenticatedProjectsStore authProjectsStore;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    public CoordinateSystemController(IASNodeClient raptorClient, ILoggerFactory logger, IAuthenticatedProjectsStore authProjectsStore)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CoordinateSystemController>();
      this.authProjectsStore = authProjectsStore;
    }

    /// <summary>
    /// Posts a coordinate system (CS) definition file to a Raptor's data model/project.
    /// </summary>
    /// <param name="request">The CS definition file structure.</param>
    /// <returns>
    /// Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>CoordinateSystemExecutorPost</executor>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [ProjectWritableVerifier]
    [ProjectWritableWithUIDVerifier]
    [System.Web.Http.Route("api/v1/coordsystem")]
    [System.Web.Http.HttpPost]
    public CoordinateSystemSettings Post([System.Web.Http.FromBody]CoordinateSystemFile request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<CoordinateSystemExecutorPost>(logger, raptorClient, null).Process(request) as CoordinateSystemSettings;
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
    [ProjectIdVerifier]
    [System.Web.Http.Route("api/v1/projects/{projectId}/coordsystem")]
    [System.Web.Http.HttpGet]
    public CoordinateSystemSettings Get([FromUri] long projectId)
    {
      ProjectID request = ProjectID.CreateProjectID(projectId);

      request.Validate();
      return RequestExecutorContainer.Build<CoordinateSystemExecutorGet>(logger, raptorClient, null).Process(request) as CoordinateSystemSettings;
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
    [ProjectUidVerifier]
    [System.Web.Http.Route("api/v2/projects/{projectUid}/coordsystem")]
    [System.Web.Http.HttpGet]
    public CoordinateSystemSettings Get([FromUri] Guid projectUid)
    {
      ProjectID request = ProjectID.CreateProjectID(ProjectID.GetProjectId(projectUid, authProjectsStore), projectUid);

      request.Validate();
      return RequestExecutorContainer.Build<CoordinateSystemExecutorGet>(logger, raptorClient, null).Process(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Posts a list of coordinates to a Raptor's data model/project for conversion.
    /// </summary>
    /// <param name="request">Description of the coordinate conversion request.</param>
    /// <returns>
    /// Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>CoordinateCoversionExecutor</executor>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [System.Web.Http.Route("api/v1/coordinateconversion")]
    [System.Web.Http.HttpPost]
    public CoordinateConversionResult Post([System.Web.Http.FromBody]CoordinateConversionRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<CoordinateConversionExecutor>(logger, raptorClient, null).Process(request) as CoordinateConversionResult;
    }
  }
}