using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Proxies;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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
      IServiceExceptionHandler serviceExceptionHandler, IProjectProxy projectProxy,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction,
      IRepository<IGeofenceEvent> geofenceRepo,
      IRepository<IProjectEvent> projectRepo)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction, "IBoundaryEvent")
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
    [HttpPut("api/v1/boundary/{ProjectUid}")]
    public async Task<GeofenceDataSingleResult> UpsertBoundary(string projectUid, [FromBody] BoundaryRequest request)
    {
      Log.LogInformation(
        $"{ToString()}.{nameof(UpsertBoundary)}: CustomerUID={CustomerUid} BoundaryRequest: {JsonConvert.SerializeObject(request)}");

      var requestFull = BoundaryRequestFull.Create(
        CustomerUid,
        IsApplication,
        await GetProject(projectUid),
        GetUserId,
        request);

      requestFull.Validate(ServiceExceptionHandler);
      requestFull.Request.BoundaryPolygonWKT = GeofenceValidation.MakeGoodWkt(requestFull.Request.BoundaryPolygonWKT);
      
      var getResult = await BoundaryHelper.GetProjectBoundaries(
        Log, ServiceExceptionHandler,
        projectUid, _projectRepository, _geofenceRepository).ConfigureAwait(false);
      if (getResult.GeofenceData.Any(g => request.Name.Equals(g.GeofenceName, StringComparison.OrdinalIgnoreCase)))
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 62);
      }

      var executor = RequestExecutorContainer.Build<UpsertBoundaryExecutor>(ConfigStore, Logger,
        ServiceExceptionHandler, _geofenceRepository, _projectRepository, ProjectProxy);
      var result = await executor.ProcessAsync(requestFull) as GeofenceDataSingleResult;

      Log.LogInformation(
        $"{ToString()}.UpsertBoundary Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Deletes a Filter Boundary from the specified project.
    /// </summary>
    /// <returns>Returns an instance of <see cref="ContractExecutionResult"/>.</returns>
    [HttpDelete("api/v1/boundary/{ProjectUid}")]
    public async Task<ContractExecutionResult> DeleteBoundary(string projectUid, [FromQuery] string boundaryUid)
    {
      Log.LogInformation(
        $"{ToString()}.DeleteBoundary: CustomerUID={CustomerUid} ProjectUid: {projectUid} BoundaryUid: {boundaryUid}");

      var requestFull = BoundaryUidRequestFull.Create(
        CustomerUid,
        IsApplication,
        await GetProject(projectUid),
        GetUserId,
        boundaryUid);

      requestFull.Validate(ServiceExceptionHandler);

      var executor = RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger,
        ServiceExceptionHandler, _geofenceRepository, _projectRepository, ProjectProxy);

      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation(
        $"{ToString()}.DeleteBoundary Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Gets the active, persistent Boundaries for a customer/project/user.
    /// </summary>
    /// <returns><see cref="GeofenceDataListResult"/></returns>
    [Route("api/v1/boundaries/{ProjectUid}")]
    [HttpGet]
    public async Task<GeofenceDataListResult> GetProjectBoundaries(
      string projectUid
      /*,    [FromServices] IGeofenceProxy geofenceProxy,
      [FromServices] IUnifiedProductivityProxy unifiedProductivityProxy*/
      )
    {
      Log.LogInformation(
        $"{ToString()}.GetProjectBoundaries: CustomerUID={CustomerUid} IsApplication={IsApplication} UserUid={GetUserId} ProjectUid: {projectUid}");

      var requestFull = BaseRequestFull.Create(
        CustomerUid,
        IsApplication,
        await GetProject(projectUid),
        GetUserId,
        Request.Headers.GetCustomHeaders());

      requestFull.Validate(ServiceExceptionHandler);

      var executor = RequestExecutorContainer.Build<GetBoundariesExecutor>(ConfigStore, Logger, ServiceExceptionHandler,
        _geofenceRepository, _projectRepository, ProjectProxy
        /* ,  geofenceProxy: geofenceProxy, unifiedProductivityProxy: unifiedProductivityProxy */ );

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
    public async Task<GeofenceDataSingleResult> GetProjectBoundary(string projectUid, [FromQuery] string boundaryUid)
    {
      Log.LogInformation(
        $"{ToString()}.GetProjectBoundary: CustomerUID={CustomerUid} IsApplication={IsApplication} UserUid={GetUserId} ProjectUid: {projectUid} BoundaryUid: {boundaryUid}");

      var requestFull = BoundaryUidRequestFull.Create(
        CustomerUid,
        IsApplication,
        await GetProject(projectUid),
        GetUserId,
        boundaryUid);

      requestFull.Validate(ServiceExceptionHandler);

      var executor = RequestExecutorContainer.Build<GetBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler,
        _geofenceRepository, _projectRepository, ProjectProxy);

      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation(
        $"{ToString()}.GetProjectBoundary Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result as GeofenceDataSingleResult;
    }

  }
}
