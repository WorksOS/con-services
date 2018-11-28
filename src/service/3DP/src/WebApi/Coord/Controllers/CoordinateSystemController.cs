using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Coord.Executors;
using VSS.Productivity3D.WebApiModels.Coord.Contracts;
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
    [PostRequestVerifier]
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
    [Route("api/v1/projects/{projectId}/coordsystem")]
    [ProjectVerifier]
    [HttpGet]
    public CoordinateSystemSettings Get([FromRoute] long projectId)
    {
      ProjectID request = new ProjectID(projectId);

      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(logger, raptorClient).Process(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Gets a coordinate system (CS) definition assigned to a Raptor's data model/project with a unique identifier.
    /// </summary>
    [Route("api/v2/projects/{projectUid}/coordsystem")]
    [ProjectVerifier]
    [HttpGet]
    public async Task<CoordinateSystemSettings> Get([FromRoute] Guid projectUid)
    {
      long projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      ProjectID request = new ProjectID(projectId, projectUid);

      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(logger, raptorClient).Process(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Posts a list of coordinates to a Raptor's data model/project for conversion.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/coordinateconversion")]
    [HttpPost]
    public CoordinateConversionResult Post([FromBody]CoordinateConversionRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(logger, raptorClient).Process(request) as CoordinateConversionResult;
    }
  }
}
