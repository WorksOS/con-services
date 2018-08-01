using System.Collections.Generic;
using System.Collections.Immutable;
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
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
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
    /// <param name="projectUid">The project uid.</param>
    /// <returns>Returns an immutable collection of <see cref="FilterDescriptor"/> objects</returns>
    [Route("api/v1/filters/{projectUid}")]
    [HttpGet]
    public async Task<FilterDescriptorListResult> GetProjectFilters(string projectUid)
    {
      Log.LogInformation($"{ToString()}.GetProjectFilters: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} isApplication={(User as TIDCustomPrincipal)?.IsApplication} UserUid={((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name} projectUid: {projectUid}");

      var user = (TIDCustomPrincipal)User;

      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(),
        user.CustomerUid,
        user.IsApplication,
        (user.Identity as GenericIdentity)?.Name,
        await (User as FilterPrincipal)?.GetProject(projectUid));

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, null, RaptorProxy);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorListResult;

      Log.LogInformation($"{ToString()}.GetProjectFilters Completed: resultCode: {result?.Code} filterCount={result?.FilterDescriptors.Count}");
      return result;
    }

    /// <summary>
    /// Get a filter by ID.
    /// </summary>
    /// <remarks>
    /// If the calling context is == Application, then get it, else get only if owned by the calling UserUid
    /// </remarks>
    /// <returns>Returns an instance of <see cref="FilterDescriptorSingleResult"/></returns>
    [Route("api/v1/filter/{ProjectUid}")]
    [HttpGet]
    public async Task<FilterDescriptorSingleResult> GetProjectFilter(string projectUid, [FromQuery] string filterUid)
    {
      Log.LogInformation($"{ToString()}.GetProjectFilter: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} IsApplication={(User as TIDCustomPrincipal)?.IsApplication} UserUid={((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name} ProjectUid: {projectUid} FilterUid: {filterUid}");

      var user = (TIDCustomPrincipal)User;

      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(),
        user.CustomerUid,
        user.IsApplication,
        (user.Identity as GenericIdentity)?.Name,
        await (User as FilterPrincipal)?.GetProject(projectUid),
        new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, ProjectListProxy, RaptorProxy);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorSingleResult;

      Log.LogInformation($"{ToString()}.GetProjectFilter Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Persistent filter is Created or Deleted/Created
    /// Transient filter is Upserted
    /// </summary>
    /// <returns>Returns an instance of <see cref="FilterDescriptorSingleResult"/></returns>
    [Route("api/v1/filter/{ProjectUid}")]
    [HttpPut]
    public async Task<FilterDescriptorSingleResult> UpsertFilter(
      [FromServices] IFileListProxy fileListProxy,
      string projectUid,
      [FromBody] FilterRequest request)
    {
      Log.LogInformation($"{ToString()}.UpsertFilter: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} FilterRequest: {JsonConvert.SerializeObject(request)}");

      if (string.IsNullOrEmpty(request?.FilterJson))
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 8, "Missing filter");
      }

      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, geofenceRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName, fileListProxy);
      var upsertFilterResult = await UpsertFilter(filterExecutor, projectUid, request);

      return upsertFilterResult;
    }

    /// <summary>
    /// Create one or more transient filters.
    /// </summary>
    /// <returns>Returns a collection of <see cref="FilterDescriptorListResult"/> which contain the created filter objects.</returns>
    [Route("api/v1/filters/{projectUid}")]
    [HttpPost]
    public async Task<FilterDescriptorListResult> CreateFilters(string projectUid, [FromBody] FilterListRequest request)
    {
      Log.LogInformation($"{ToString()}.CreateFilters: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} FilterListRequest: {JsonConvert.SerializeObject(request)}");

      if (request?.FilterRequests == null || request.FilterRequests?.Count() == 0)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7, "Missing filters");
      }


      //Only transient filters for now. Supporting batching of permanent filters requires rollback logic when one or more fails.
      foreach (var filterRequest in request.FilterRequests)
      {
        if (filterRequest.FilterType != FilterType.Transient)
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 24);
        }

        if (!string.IsNullOrEmpty(filterRequest.FilterUid))
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 16);
        }
      }

      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, geofenceRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);

      var newFilters = new List<FilterDescriptor>();

      foreach (var filterRequest in request.FilterRequests)
      {
        var upsertFilterResult = await UpsertFilter(filterExecutor, projectUid, filterRequest);
        newFilters.Add(upsertFilterResult.FilterDescriptor);
      }
      var result = new FilterDescriptorListResult { FilterDescriptors = newFilters.ToImmutableList() };
      Log.LogInformation($"{ToString()}.CreateFilters Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Validates and saves a single filter. Also creates the project-geofence association if the filter contains a polygon boundary.
    /// </summary>
    private async Task<FilterDescriptorSingleResult> UpsertFilter(UpsertFilterExecutor filterExecutor, string projectUid, FilterRequest filterRequest)
    {
      var user = (TIDCustomPrincipal)User;

      var requestFull = FilterRequestFull.Create(
        Request.Headers.GetCustomHeaders(),
        user.CustomerUid,
        user.IsApplication,
        (user.Identity as GenericIdentity)?.Name,
        await (User as FilterPrincipal)?.GetProject(projectUid),
        filterRequest);

      requestFull.Validate(ServiceExceptionHandler);

      var upsertFilterResult = await filterExecutor.ProcessAsync(requestFull).ConfigureAwait(false) as FilterDescriptorSingleResult;

      Log.LogInformation($"{ToString()}.UpsertFilter Completed: resultCode: {upsertFilterResult?.Code} result: {JsonConvert.SerializeObject(upsertFilterResult)}");
      return upsertFilterResult;
    }

    /// <summary>
    /// Deletes a Filter from the specified project.
    /// </summary>
    /// <returns><see cref="ContractExecutionResult"/></returns>
    [Route("api/v1/filter/{ProjectUid}")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeleteFilter(string projectUid, [FromQuery] string filterUid)
    {
      Log.LogInformation($"{ToString()}.DeleteFilter: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} ProjectUid: {projectUid} FilterUid: {filterUid}");

      var user = (TIDCustomPrincipal)User;

      var customHeaders = Request.Headers.GetCustomHeaders();
      var requestFull = FilterRequestFull.Create(
        customHeaders,
        user.CustomerUid,
        user.IsApplication,
        (user.Identity as GenericIdentity)?.Name,
        await (User as FilterPrincipal)?.GetProject(projectUid),
        new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler, true);

      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, filterRepo, null, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation($"{ToString()}.DeleteFilter Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }
  }
}
