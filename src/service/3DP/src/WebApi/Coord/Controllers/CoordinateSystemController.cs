using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.WebApi.Models.Coord.Contracts;
using VSS.Productivity3D.WebApi.Models.Coord.Executors;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Coord.Controllers
{
  /// <summary>
  /// Controller for the CoordinateSystemFile resource.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CoordinateSystemController : Controller, ICoordinateSystemFileContract
  {
    private readonly ILogger _log;
    private readonly ILoggerFactory logger;
    private readonly IConfigurationStore configStore;
    private readonly ITRexCompactionDataProxy trexCompactionDataProxy;

    protected IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public CoordinateSystemController(ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy)
    {
      this.logger = logger;
      _log = logger.CreateLogger<CoordinateSystemController>();
      this.configStore = configStore;
      this.trexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Posts a coordinate system (CS) definition file to a Raptor's data model/project.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/coordsystem")]
    [Route("api/v2/coordsystem")]
    [Obsolete("TBC doesn't call this (as per Shawn 20200826")]
    [HttpPost]
    public async Task<CoordinateSystemSettings> Post([FromBody] CoordinateSystemFile request)
    {
      request.Validate();

      return await RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(logger,
        configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Posts a coordinate system (CS) definition file to a Raptor for validation.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/coordsystem/validation")]
    [Route("api/v2/coordsystem/validation")]
    [HttpPost]
    public async Task<CoordinateSystemSettings> PostValidate([FromBody] CoordinateSystemFileValidationRequest request)
    {
      var serializedRequest = JsonConvert.SerializeObject(request);
      _log.LogDebug($"POST coordsystem/validation: request {serializedRequest}");

      request.Validate();

      return await RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(logger,
        configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Gets a coordinate system (CS) definition assigned to a Raptor's data model/project.
    /// </summary>
    [ProjectVerifier]
    [HttpGet("api/v1/projects/{projectId}/coordsystem")]
    public async Task<CoordinateSystemSettings> Get([FromRoute] long projectId)
    {
      var projectUid = await ((RaptorPrincipal)User).GetProjectUid(projectId);
      var request = new ProjectID(projectId, projectUid);

      request.Validate();

      return await RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(logger,
        configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Gets a coordinate system (CS) definition assigned to a Raptor's data model/project with a unique identifier.
    /// </summary>
    [ProjectVerifier]
    [HttpGet("api/v2/projects/{projectUid}/coordsystem")]
    public async Task<CoordinateSystemSettings> Get([FromRoute] Guid projectUid)
    {
      long projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var request = new ProjectID(projectId, projectUid);

      request.Validate();

      return await RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(logger,
        configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as CoordinateSystemSettings;
    }

    /// <summary>
    /// Posts a list of coordinates to a Raptor's data model/project for conversion.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/coordinateconversion")]
    [Route("api/v2/coordinateconversion")]
    [HttpPost]
    public async Task<CoordinateConversionResult> Post([FromBody] CoordinateConversionRequest request)
    {
      request.Validate();

      return await RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(logger,
        configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as CoordinateConversionResult;
    }
  }
}
