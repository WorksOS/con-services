using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Filters.Authentication;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.WebAPI.Controllers
{
  /// <summary>
  /// Geofence boundary controller
  /// </summary>
  public class BoundaryController : BaseController
  {
    private readonly GeofenceRepository _geofenceRepository;
    private readonly ProjectRepository _projectRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundaryController"/> class.
    /// </summary>
    public BoundaryController(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IRepository<IGeofenceEvent> geofenceRepo,
      IKafka producer, IRepository<IProjectEvent> projectRepo)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, producer, "IBoundaryEvent")
    {
      Log = logger.CreateLogger<BoundaryController>();
      _geofenceRepository = geofenceRepo as GeofenceRepository;
      _projectRepository = projectRepo as ProjectRepository;
    }

    /// <summary>
    /// Create a new Filter Boundary.
    /// </summary>
    /// <param name="projectUid">The Project Uid.</param>
    /// <param name="request">A serialized <see cref="BoundaryRequest"/> instance.</param>
    /// <returns>Returns an instance of <see cref="GeofenceDataSingleResult"/> for type <see cref="VSS.MasterData.Models.Models.GeofenceData"/>.</returns>
    [Route("api/v1/boundary/{ProjectUid}")]
    [HttpPut]
    public async Task<GeofenceDataSingleResult> UpsertBoundary(string projectUid, [FromBody] BoundaryRequest request)
    {
      Log.LogInformation(
        $"{ToString()}.UpsertBoundary: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} BoundaryRequest: {JsonConvert.SerializeObject(request)}");

      var requestFull = BoundaryRequestFull.Create(
        (User as TIDCustomPrincipal)?.CustomerUid,
        (User as TIDCustomPrincipal).IsApplication,
        await (User as FilterPrincipal).GetProject(projectUid),
        ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
        request);

      requestFull.Validate(ServiceExceptionHandler);
      requestFull.Request.BoundaryPolygonWKT = GeofenceValidation.MakeGoodWkt(requestFull.Request.BoundaryPolygonWKT);

      var getResult = await GetProjectBoundaries(projectUid);
      if (getResult.GeofenceData.Any(g => request.Name.Equals(g.GeofenceName, StringComparison.OrdinalIgnoreCase)))
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 62);
      }

      var executor = RequestExecutorContainer.Build<UpsertBoundaryExecutor>(ConfigStore, Logger,
        ServiceExceptionHandler, _geofenceRepository, _projectRepository, ProjectListProxy, RaptorProxy, Producer,
        KafkaTopicName);
      var result = await executor.ProcessAsync(requestFull) as GeofenceDataSingleResult;

      Log.LogInformation(
        $"{ToString()}.UpsertBoundary Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Deletes a Filter Boundary from the specified project.
    /// </summary>
    /// <returns>Returns an instance of <see cref="ContractExecutionResult"/>.</returns>
    [Route("api/v1/boundary/{ProjectUid}")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeleteBoundary(string projectUid, [FromBody] string boundaryUid)
    {
      Log.LogInformation(
        $"{ToString()}.DeleteBoundary: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} ProjectUid: {projectUid} BoundaryUid: {boundaryUid}");

      var requestFull = BoundaryUidRequestFull.Create(
        (User as TIDCustomPrincipal)?.CustomerUid,
        (User as TIDCustomPrincipal).IsApplication,
        await (User as FilterPrincipal).GetProject(projectUid),
        ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
        boundaryUid);

      requestFull.Validate(ServiceExceptionHandler);

      var executor = RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger,
        ServiceExceptionHandler, _geofenceRepository, _projectRepository, ProjectListProxy, RaptorProxy, Producer,
        KafkaTopicName);

      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation(
        $"{ToString()}.DeleteBoundary Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Gets the active, persistent Boundaries for a customer/project/user.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns><see cref="GeofenceDataListResult"/></returns>
    [Route("api/v1/boundaries/{ProjectUid}")]
    [HttpGet]
    public async Task<GeofenceDataListResult> GetProjectBoundaries(string projectUid)
    {
      Log.LogInformation(
        $"{ToString()}.GetProjectBoundaries: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} IsApplication={(User as TIDCustomPrincipal)?.IsApplication} UserUid={((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name} ProjectUid: {projectUid}");

      var requestFull = BaseRequestFull.Create(
        (User as TIDCustomPrincipal)?.CustomerUid,
        (User as TIDCustomPrincipal).IsApplication,
        await (User as FilterPrincipal).GetProject(projectUid),
        ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name);

      requestFull.Validate(ServiceExceptionHandler);

      var executor = RequestExecutorContainer.Build<GetBoundariesExecutor>(ConfigStore, Logger, ServiceExceptionHandler,
        _geofenceRepository, _projectRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);

      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation(
        $"{ToString()}.GetProjectBoundaries Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result as GeofenceDataListResult;
    }

    /// <summary>
    /// Gets the active, persistent Boundary for a customer/project/user.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="boundaryUid">The boundary uid.</param>
    /// <returns><see cref="GeofenceDataSingleResult"/></returns>
    [Route("api/v1/boundary/{ProjectUid}")]
    [HttpGet]
    public async Task<GeofenceDataSingleResult> GetProjectBoundary(string projectUid, [FromBody] string boundaryUid)
    {
      Log.LogInformation(
        $"{ToString()}.GetProjectBoundary: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} IsApplication={(User as TIDCustomPrincipal)?.IsApplication} UserUid={((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name} ProjectUid: {projectUid} BoundaryUid: {boundaryUid}");

      var requestFull = BoundaryUidRequestFull.Create(
        (User as TIDCustomPrincipal)?.CustomerUid,
        (User as TIDCustomPrincipal).IsApplication,
        await (User as FilterPrincipal).GetProject(projectUid),
        ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
        boundaryUid);

      requestFull.Validate(ServiceExceptionHandler);

      var executor = RequestExecutorContainer.Build<GetBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler,
        _geofenceRepository, _projectRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);

      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation(
        $"{ToString()}.GetProjectBoundary Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result as GeofenceDataSingleResult;
    }
  }
}
