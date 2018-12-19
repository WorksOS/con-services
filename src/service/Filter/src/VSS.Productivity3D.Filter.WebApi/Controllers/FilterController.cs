using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
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
  /// FilterController controller
  /// </summary>
  public class FilterController : BaseController
  {
    private readonly GeofenceRepository geofenceRepository;
    private readonly FilterRepository filterRepo;

    private static Task<ProjectData> GetProjectForUser(ClaimsPrincipal user, string projectUid) => (user as FilterPrincipal)?.GetProject(projectUid);

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterController"/> class.
    /// </summary>
    public FilterController(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IRepository<IFilterEvent> filterRepo,
      IKafka producer, IRepository<IGeofenceEvent> geofenceRepo)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, producer, "IFilterEvent")
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
      Log.LogInformation($"{nameof(GetProjectFilters)}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} isApplication={(User as TIDCustomPrincipal)?.IsApplication} UserUid={((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name} projectUid: {projectUid}");

      var user = (TIDCustomPrincipal)User;

      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(),
        user.CustomerUid,
        user.IsApplication,
        (user.Identity as GenericIdentity)?.Name,
        await GetProjectForUser(User, projectUid));

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
      Log.LogInformation($"{nameof(GetProjectFilter)}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} IsApplication={(User as TIDCustomPrincipal)?.IsApplication} UserUid={((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name} ProjectUid: {projectUid} FilterUid: {filterUid}");

      var user = (TIDCustomPrincipal)User;

      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(),
        user.CustomerUid,
        user.IsApplication,
        (user.Identity as GenericIdentity)?.Name,
        await GetProjectForUser(User, projectUid),
        new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, ProjectListProxy, RaptorProxy);
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
      [FromServices] IFileListProxy fileListProxy,
      string projectUid,
      [FromBody] FilterRequest request)
    {
      Log.LogInformation($"{nameof(PutFilter)}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} FilterRequest: {JsonConvert.SerializeObject(request)}");

      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, geofenceRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName, fileListProxy);
      var upsertFilterResult = await UpsertFilter((TIDCustomPrincipal)User, (User.Identity as GenericIdentity)?.Name, filterExecutor, await GetProjectForUser(User, projectUid), request);

      return upsertFilterResult;
    }

    /// <summary>
    /// Create one or more transient filters.
    /// </summary>
    /// <remarks>
    /// Only transient filters for now. Supporting batching of permanent filters requires rollback logic when one or more fails.
    /// </remarks>
    [HttpPost("api/v1/filters/{projectUid}")]
    public async Task<FilterDescriptorListResult> CreateFilters(string projectUid, [FromBody] FilterListRequest request)
    {
      Log.LogInformation($"{nameof(CreateFilters)}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} FilterListRequest: {JsonConvert.SerializeObject(request)}");

      if (request?.FilterRequests == null || request.FilterRequests?.Count() == 0)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7, "Missing filters");
      }

      var project = await GetProjectForUser(User, projectUid);
      var newFilters = new List<FilterDescriptor>();
      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, geofenceRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var username = (User.Identity as GenericIdentity)?.Name;

      foreach (var filterRequest in request.FilterRequests)
      {
        newFilters.Add((await UpsertFilter((TIDCustomPrincipal)User, username, filterExecutor, project, filterRequest)).FilterDescriptor);
      }

      var result = new FilterDescriptorListResult{ FilterDescriptors = newFilters.ToImmutableList() };

      Log.LogInformation($"{nameof(CreateFilters)} Completed: resultCode: {result.Code} result: {JsonConvert.SerializeObject(result)}");

      return result;
    }

    /// <summary>
    /// Deletes a Filter from the specified project.
    /// </summary>
    [HttpDelete("api/v1/filter/{ProjectUid}")]
    public async Task<ContractExecutionResult> DeleteFilter(string projectUid, [FromQuery] string filterUid)
    {
      Log.LogInformation($"{nameof(DeleteFilter)}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} ProjectUid: {projectUid} FilterUid: {filterUid}");

      var user = (TIDCustomPrincipal)User;

      var customHeaders = Request.Headers.GetCustomHeaders();
      var requestFull = FilterRequestFull.Create(
        customHeaders,
        user.CustomerUid,
        user.IsApplication,
        (user.Identity as GenericIdentity)?.Name,
        await GetProjectForUser(User, projectUid),
        new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation($"{nameof(DeleteFilter)} Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Validates and saves a single filter. Also creates the project-geofence association if the filter contains a polygon boundary.
    /// </summary>
    private async Task<FilterDescriptorSingleResult> UpsertFilter(TIDCustomPrincipal user, string username, UpsertFilterExecutor filterExecutor, ProjectData project, FilterRequest filterRequest)
    {
      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(),
        user.CustomerUid,
        user.IsApplication,
        username,
        project,
        filterRequest);

      requestFull.Validate(ServiceExceptionHandler);

      return await filterExecutor.ProcessAsync(requestFull) as FilterDescriptorSingleResult;
    }
  }
}
