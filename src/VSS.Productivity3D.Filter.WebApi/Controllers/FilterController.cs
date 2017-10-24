using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Immutable;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Validators;
using VSS.Productivity3D.Filter.WebApi.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Filter.WebAPI.Controllers
{
  /// <summary>
  /// FilterController controller
  /// </summary>
  public class FilterController : BaseController
  {
    private readonly GeofenceRepository GeofenceRepository;
    private readonly FilterRepository FilterRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterController"/> class.
    /// </summary>
    public FilterController(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IRepository<IFilterEvent> filterRepo, 
      IKafka producer, IRepository<IGeofenceEvent> geofenceRepo)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, producer, "IFilterEvent")
    {
      Log = logger.CreateLogger<FilterController>();
      FilterRepo = filterRepo as FilterRepository;
      GeofenceRepository = geofenceRepo as GeofenceRepository;
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
      var requestFull =
        FilterRequestFull.Create(
          (User as TIDCustomPrincipal)?.CustomerUid,
          (User as TIDCustomPrincipal).IsApplication,
          ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
          projectUid);

      requestFull.Validate(ServiceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorListResult;

      Log.LogInformation($"{ToString()}.GetProjectFilters Completed: resultCode: {result?.Code} filterCount={result?.FilterDescriptors.Count}");
      return result;
    }

    /// <summary>
    /// Gets the filter requested
    ///    If the calling context is == Application, then get it, 
    ///       else get only if owned by the calling UserUid
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filterUid">The project uid.</param>
    /// <returns>Returns an instance of <see cref="FilterDescriptorSingleResult"/></returns>
    [Route("api/v1/filter/{ProjectUid}")]
    [HttpGet]
    public async Task<FilterDescriptorSingleResult> GetProjectFilter(string projectUid, [FromUri] string filterUid)
    {
      Log.LogInformation($"{ToString()}.GetProjectFilter: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} IsApplication={(User as TIDCustomPrincipal)?.IsApplication} UserUid={((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name} ProjectUid: {projectUid} FilterUid: {filterUid}");
      var requestFull =
        FilterRequestFull.Create(
          (User as TIDCustomPrincipal)?.CustomerUid,
          (User as TIDCustomPrincipal).IsApplication,
          ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
          projectUid,
          new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorSingleResult;

      Log.LogInformation($"{ToString()}.GetProjectFilter Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Persistent filter is Created or Deleted/Created
    /// Transient filter is Upserted
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="request"></param>
    /// <returns>Returns an instance of <see cref="FilterDescriptorSingleResult"/></returns>
    [Route("api/v1/filter/{ProjectUid}")]
    [HttpPut]
    public async Task<FilterDescriptorSingleResult> UpsertFilter(string projectUid, [FromBody] FilterRequest request)
    {
      Log.LogInformation($"{ToString()}.UpsertFilter: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} FilterRequest: {JsonConvert.SerializeObject(request)}");

      if (request == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 8, "Missing filter");
      }

      await ValidationUtil.ValidateProjectForCustomer(ProjectListProxy, Log, ServiceExceptionHandler, Request.Headers.GetCustomHeaders(),
        (User as TIDCustomPrincipal)?.CustomerUid, projectUid).ConfigureAwait(false);

      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);

      var upsertFilterResult = await UpsertFilter(filterExecutor, projectUid, request);

      return upsertFilterResult;
    }

    /// <summary>
    /// List of transient filters
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="request"></param>
    /// <returns>FilterDescriptorListResult</returns>
    [Route("api/v1/filters/{projectUid}")]
    [HttpPost]
    public async Task<FilterDescriptorListResult> CreateFilters(string projectUid, [FromBody] FilterListRequest request)
    {
      Log.LogInformation($"{ToString()}.CreateFilters: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} FilterListRequest: {JsonConvert.SerializeObject(request)}");

      if (request == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7, "Missing filters");
      }

      await ValidationUtil.ValidateProjectForCustomer(ProjectListProxy, Log, ServiceExceptionHandler, Request.Headers.GetCustomHeaders(),
              (User as TIDCustomPrincipal)?.CustomerUid, projectUid).ConfigureAwait(false);

      //Only transient filters for now. Supporting batching of permanent filters requires rollback logic when one or more fails.
      foreach (var filterRequest in request.FilterRequests)
      {
        if (!string.IsNullOrEmpty(filterRequest.Name))
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 24);
        }

        if (!string.IsNullOrEmpty(filterRequest.FilterUid))
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 16);
        }
      }

      var filterExecutor = RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepository, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);

      var newFilters = new List<FilterDescriptor>();

      foreach (var filterRequest in request.FilterRequests)
      {
        var upsertFilterResult = await UpsertFilter(filterExecutor, projectUid, filterRequest);
        newFilters.Add(upsertFilterResult.FilterDescriptor);
      }
      var result = new FilterDescriptorListResult() { FilterDescriptors = newFilters.ToImmutableList() };
      Log.LogInformation($"{ToString()}.CreateFilters Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Validates and saves a single filter. Also creates the project-geofence association if the filter contains a polygon boundary.
    /// </summary>
    /// <param name="filterExecutor"></param>
    /// <param name="projectUid"></param>
    /// <param name="filterRequest"></param>
    /// <returns></returns>
    private async Task<FilterDescriptorSingleResult> UpsertFilter(UpsertFilterExecutor filterExecutor, string projectUid, FilterRequest filterRequest)
    {
      var requestFull = FilterRequestFull.Create(
       (User as TIDCustomPrincipal)?.CustomerUid,
       (User as TIDCustomPrincipal).IsApplication,
       ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
       projectUid,
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
    public async Task<ContractExecutionResult> DeleteFilter(string projectUid, [FromUri] string filterUid)
    {
      Log.LogInformation($"{ToString()}.DeleteFilter: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} ProjectUid: {projectUid} FilterUid: {filterUid}");

      var requestFull = FilterRequestFull.Create(
        (User as TIDCustomPrincipal)?.CustomerUid,
        (User as TIDCustomPrincipal).IsApplication,
        ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
        projectUid,
        new FilterRequest { FilterUid = filterUid });

      requestFull.Validate(ServiceExceptionHandler);

      await ValidationUtil.ValidateProjectForCustomer(ProjectListProxy, Log, ServiceExceptionHandler, Request.Headers.GetCustomHeaders(),
        (User as TIDCustomPrincipal)?.CustomerUid, projectUid).ConfigureAwait(false);

      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(requestFull);

      Log.LogInformation($"{ToString()}.DeleteFilter Completed: resultCode: {result?.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }
  }
}