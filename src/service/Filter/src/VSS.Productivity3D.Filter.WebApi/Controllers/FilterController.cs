using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.WebAPI.Controllers
{
  /// <summary>
  /// FilterController controller
  /// </summary>
  public class FilterController : BaseController
  {
    private readonly GeofenceRepository geofenceRepository;
    private readonly FilterRepository filterRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterController"/> class.
    /// </summary>
    public FilterController(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy, IRaptorProxy raptorProxy, IRepository<IFilterEvent> filterRepo,
      IKafka producer, IRepository<IGeofenceEvent> geofenceRepo)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, raptorProxy, producer, "IFilterEvent")
    {
      Log = logger.CreateLogger<FilterController>();
      this.filterRepo = filterRepo as FilterRepository;
      geofenceRepository = geofenceRepo as GeofenceRepository;
    }

    /// <summary>
    /// Gets the active, persistent filters for a customer/project/user.
    /// </summary>
    [HttpGet("api/v1/filters/{projectUid}")]
    public async Task<FilterDescriptorListResult> GetProjectFilters(string projectUid)
    {
      Log.LogInformation($"{nameof(GetProjectFilters)}: CustomerUID={CustomerUid} isApplication={IsApplication} UserUid={GetUserId} projectUid: {projectUid}");

      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(true),
        CustomerUid,
        IsApplication,
        GetUserId,
        await GetProject(projectUid));

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, null, RaptorProxy);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorListResult;

      Log.LogInformation($"{nameof(GetProjectFilters)} Completed: resultCode: {result?.Code} filterCount={result?.FilterDescriptors.Count}");
      return result;
    }

    /// <summary>
    /// Get a filter by ID.
    /// </summary>
    /// <remarks>
    /// If the calling context is == Application, then get it, else get only if owned by the calling UserUid
    /// </remarks>
    [HttpGet("api/v1/filter/{ProjectUid}")]
    public async Task<FilterDescriptorSingleResult> GetProjectFilter(string projectUid, [FromQuery] string filterUid)
    {
      Log.LogInformation($"{nameof(GetProjectFilter)}: CustomerUID={CustomerUid} IsApplication={IsApplication} UserUid={GetUserId} ProjectUid: {projectUid} FilterUid: {filterUid}");

      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(true),
        CustomerUid,
        IsApplication,
        GetUserId,
        await GetProject(projectUid),
        new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, ProjectProxy, RaptorProxy);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorSingleResult;

      Log.LogInformation($"{nameof(GetProjectFilter)} Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Persistent filter is Created or Deleted/Created
    /// Transient filter is Upserted
    /// </summary>
    [HttpPut("api/v1/filter/{ProjectUid}")]
    public async Task<FilterDescriptorSingleResult> PutFilter(
      [FromServices] IGeofenceProxy geofenceProxy,
      [FromServices] IFileImportProxy fileImportProxy,
      [FromServices] INotificationHubClient notificationHubClient,
      string projectUid,
      [FromBody] FilterRequest request)
    {
      Log.LogInformation($"{nameof(PutFilter)}: CustomerUID={CustomerUid} FilterRequest: {JsonConvert.SerializeObject(request)}");

      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, geofenceRepository, ProjectProxy, RaptorProxy, Producer, KafkaTopicName, fileImportProxy, geofenceProxy);
      var upsertFilterResult = await UpsertFilter(filterExecutor, await GetProject(projectUid), request);

      if (upsertFilterResult.FilterDescriptor.FilterType == FilterType.Persistent)
      {
        await notificationHubClient.Notify(new ProjectChangedNotification(Guid.Parse(projectUid)));
      }

      return upsertFilterResult;
    }

    /// <summary>
    /// Create one or more transient filters.
    /// </summary>
    /// <remarks>
    /// Only transient filters for now. Supporting batching of permanent filters requires rollback logic when one or more fails.
    /// </remarks>
    [HttpPost("api/v1/filters/{projectUid}")]
    public async Task<FilterDescriptorListResult> CreateFilters(
      string projectUid, 
      [FromBody] FilterListRequest request,
      [FromServices] IGeofenceProxy geofenceProxy)
    {
      Log.LogInformation($"{nameof(CreateFilters)}: CustomerUID={CustomerUid} FilterListRequest: {JsonConvert.SerializeObject(request)}");

      if (request?.FilterRequests == null || request.FilterRequests?.Count() == 0)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7, "Missing filters");
      }
    
      var projectTask = GetProject(projectUid);
      var newFilters = new List<FilterDescriptor>();
      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, geofenceRepository, ProjectProxy, RaptorProxy, Producer, KafkaTopicName, null, geofenceProxy);

      var project = await projectTask;

      foreach (var filterRequest in request.FilterRequests)
      {
        newFilters.Add((await UpsertFilter(filterExecutor, project, filterRequest)).FilterDescriptor);
      }

      var result = new FilterDescriptorListResult { FilterDescriptors = newFilters.ToImmutableList() };

      Log.LogInformation($"{nameof(CreateFilters)} Completed: resultCode: {result.Code} result: {JsonConvert.SerializeObject(result)}");

      return result;
    }

    /// <summary>
    /// Deletes a Filter from the specified project.
    /// </summary>
    [HttpDelete("api/v1/filter/{ProjectUid}")]
    public async Task<ContractExecutionResult> DeleteFilter(string projectUid, [FromQuery] string filterUid)
    {
      Log.LogInformation($"{nameof(DeleteFilter)}: CustomerUID={CustomerUid} ProjectUid: {projectUid} FilterUid: {filterUid}");

      var customHeaders = Request.Headers.GetCustomHeaders();
      var requestFull = FilterRequestFull.Create(
        customHeaders,
        CustomerUid,
        IsApplication,
        GetUserId,
        await GetProject(projectUid),
        new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, ProjectProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation($"{nameof(DeleteFilter)} Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Validates and saves a single filter. Also creates the project-geofence association if the filter contains a polygon boundary.
    /// </summary>
    private async Task<FilterDescriptorSingleResult> UpsertFilter(UpsertFilterExecutor filterExecutor, ProjectData project, FilterRequest filterRequest)
    {
      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(true),
        CustomerUid,
        IsApplication,
        GetUserId,
        project,
        filterRequest);

      requestFull.Validate(ServiceExceptionHandler);

      return await filterExecutor.ProcessAsync(requestFull) as FilterDescriptorSingleResult;
    }
  }
}
