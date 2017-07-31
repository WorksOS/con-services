using System;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Repositories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.ConfigurationStore;
using VSS.Common.Exceptions;
using VSS.KafkaConsumer.Kafka;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Productivity3D.Filter.WebApi.Filters;
using Newtonsoft.Json;
using VSS.Productivity3D.Filter.Common.Executors;
using System.Threading.Tasks;
using System.Web.Http;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace VSS.Productivity3D.Filter.WebAPI.Controllers
{
  /// <summary>
  /// FilterController controller
  /// </summary>
  public class FilterController : Controller
  {
    private FilterRepository filterRepo;
    private IProjectListProxy projectListProxy;
    private IConfigurationStore configStore;
    private ILoggerFactory logger;
    private ILogger log;
    private IServiceExceptionHandler serviceExceptionHandler;
    private IKafka producer;
    private string kafkaTopicName;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterController"/> class.
    /// </summary>
    public FilterController(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, 
      IProjectListProxy projectListProxy, IRepository<IFilterEvent> filterRepo, IKafka producer)
    {
      this.configStore = configStore;
      this.logger = logger;
      log = logger.CreateLogger<FilterController>();
      this.serviceExceptionHandler = serviceExceptionHandler;

      this.projectListProxy = projectListProxy;
      this.filterRepo = filterRepo as FilterRepository;

      this.producer = producer;
      if (!this.producer.IsInitializedProducer)
        this.producer.InitProducer(configStore);

      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IFilterEvent" +
                       configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }


    /// <summary>
    /// Gets the filters for a project.
    ///    If the calling context is == Application, then get all !deleted, 
    ///       else get only those for the calling UserUid
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns>FilterDescriptorListResult</returns>
    [Route("api/v1/filters/{projectUid}")]
    [HttpGet]
    public async Task<FilterDescriptorListResult> GetProjectFilters(string projectUid)
    {
      Guid projectUidGuid;
      if (string.IsNullOrEmpty(projectUid) || Guid.TryParse(projectUid, out projectUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      var tidCustomPrincipal = User as TIDCustomPrincipal;
      log.LogInformation($"{ToString()}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} isApplication={tidCustomPrincipal != null && tidCustomPrincipal.isApplication} UserUid={((User as TIDCustomPrincipal).Identity as GenericIdentity).Name} projectUid: {projectUid}");

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest((User as TIDCustomPrincipal)?.CustomerUid,
          (User as TIDCustomPrincipal).isApplication, ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
          projectUid);
      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler, filterRepo);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorListResult;

      log.LogInformation($"{ToString()} Completed: resultCode: {result.Code} filterCount={result.filterDescriptors.Count}");
      return result;
    }

    /// <summary>
    /// Gets the filter requested
    ///    If the calling context is == Application, then get it, 
    ///       else get only if owned by the calling UserUid
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filterUid">The project uid.</param>
    /// <returns>FilterDescriptorSingleResult</returns>
    [Route("api/v1/filter/{projectUid}")]
    [HttpGet]
    public async Task<FilterDescriptorSingleResult> GetProjectFilter(string projectUid, [FromUri] string filterUid)
    {
      Guid projectUidGuid;
      Guid filterUidGuid;
      if (string.IsNullOrEmpty(projectUid) || Guid.TryParse(projectUid, out projectUidGuid) == false 
          || string.IsNullOrEmpty(filterUid) || Guid.TryParse(filterUid, out filterUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      var tidCustomPrincipal = User as TIDCustomPrincipal;
      log.LogInformation($"{ToString()}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} isApplication={tidCustomPrincipal != null && tidCustomPrincipal.isApplication} UserUid={((User as TIDCustomPrincipal).Identity as GenericIdentity).Name} projectUid: {projectUid} filterUid: {filterUid}");

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest((User as TIDCustomPrincipal)?.CustomerUid,
          (User as TIDCustomPrincipal).isApplication, ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
          projectUid, filterUid);
      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo);
      var result = (await executor.ProcessAsync(requestFull)) as FilterDescriptorSingleResult;
      
      log.LogInformation($"{ToString()} Completed: resultCode: {result.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Persistant filter is Created or Deleted/Created
    /// Transient filter is Upserted
    /// </summary>
    /// <param name="request"></param>
    /// <returns>FilterDescriptorSingleResult</returns>
    [Route("api/v4/filter/{projectUid}")]
    [HttpPut]
    public async Task<FilterDescriptorSingleResult> UpsertFilter(string projectUid, [FromBody] FilterRequest request)
    {
      Guid projectUidGuid;
      Guid filterUiddGuid; // if supplied then it must be valid
      if (request == null ||
          string.IsNullOrEmpty(projectUid) || Guid.TryParse(projectUid, out projectUidGuid) == false ||
          (!string.IsNullOrEmpty(request.filterUid) && Guid.TryParse(request.filterUid, out filterUiddGuid) == false))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      log.LogInformation($"{ToString()}: CustomerUID={(User as TIDCustomPrincipal)?.CustomerUid} request: {JsonConvert.SerializeObject(request)} FilterRequest: {JsonConvert.SerializeObject(request)}");

      await FilterValidation.ValidateCustomerProject(projectListProxy, log, serviceExceptionHandler, Request.Headers.GetCustomHeaders(),
        (User as TIDCustomPrincipal)?.CustomerUid, projectUid).ConfigureAwait(false);
      
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest((User as TIDCustomPrincipal)?.CustomerUid,
          (User as TIDCustomPrincipal).isApplication, ((User as TIDCustomPrincipal)?.Identity as GenericIdentity)?.Name,
          projectUid, request.filterUid, request.name, request.filterJson);
      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(requestFull) as FilterDescriptorSingleResult;

      log.LogInformation($"{this.ToString()} Completed: resultCode: {result.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Upserts the project settings for a project.
    /// </summary>
    /// <returns>FilterDescriptorSingleResult</returns>
    [Route("api/v4/filter/{projectUid}")]
    [HttpDelete]
    public async Task<FilterDescriptorSingleResult> DeleteFilter(string projectUid, [FromUri] string filterUid)
    {
      if (string.IsNullOrEmpty(filterUid))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      log.LogInformation($"{this.ToString()}: CustomerUID={(User as TIDCustomPrincipal).CustomerUid} projectUid: {projectUid} filterUid: {filterUid}");

      // todo validate request; cust/project; and filterUid/userUid

      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(filterUid) as FilterDescriptorSingleResult;

      log.LogInformation($"{this.ToString()} Completed: resultCode: {result.Code} result: {JsonConvert.SerializeObject(result)}");
      return result;
    }
  }
}
