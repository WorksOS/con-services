using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Settings controller, version 4.
  /// </summary>
  public class ProjectSettingsV4Controller : BaseController
  {
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    //private readonly IConfigurationStore configStore;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="subscriptionProxy"></param>
    /// <param name="projectRepo">The MasterData ProjectRepository persistent storage interface</param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy">The Raptor Proxy refernce</param>
    /// <param name="configStore">configStore for module meta data</param>
    /// <param name="logger">The ILoggerFactory logging implementation</param>
    /// <param name="serviceExceptionHandler">The service exception handler</param>
    /// <param name="producer">The Kafka consumer</param>
    /// <param name="subscriptionsRepo"></param>
    /// <param name="requestFactory"></param>
    public ProjectSettingsV4Controller(ILoggerFactory logger, IConfigurationStore configStore, 
      IServiceExceptionHandler serviceExceptionHandler, IKafka producer,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ISubscriptionProxy subscriptionProxy,
      IRepository<IProjectEvent> projectRepo, IRepository<ISubscriptionEvent> subscriptionsRepo,
      IRequestFactory requestFactory
      )
      : base(logger.CreateLogger<BaseController>(), configStore, serviceExceptionHandler, producer, 
          geofenceProxy, raptorProxy, subscriptionProxy,
          projectRepo, subscriptionsRepo)
    {
      this.logger = logger;
      log = logger.CreateLogger<ProjectSettingsV4Controller>();
      this.requestFactory = requestFactory;
    }


    /// <summary>
    /// Gets the project settings for a project.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    [Route("api/v4/projectsettings/{projectUid}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettings(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("GetProjectSettings", projectUid);

      var projectSettingsRequest = requestFactory.Create<ProjectSettingsRequestHelper>(r => r
          .CustomerUid(customerUid))
        .CreateProjectSettingsRequest(projectUid, "");
      projectSettingsRequest.Validate();
     
      var result = (await WithServiceExceptionTryExecuteAsync(() => 
        RequestExecutorContainerFactory
        .Build<GetProjectSettingsExecutor>(logger, configStore, serviceExceptionHandler, 
                customerUid, null, null, null,
                producer, kafkaTopicName,
                null, raptorProxy, null,
                projectRepo)
        .ProcessAsync(projectSettingsRequest) 
        )) as ProjectSettingsResult;
     
      log.LogResult(this.ToString(), projectUid, result);
      return result;
    }


    /// <summary>
    /// Upserts the project settings for a project.
    /// </summary>
    /// <returns></returns>
    [Route("api/v4/projectsettings")]
    [HttpPut]
    public async Task<ProjectSettingsResult> UpsertProjectSettings([FromBody]ProjectSettingsRequest request)
    {
      if (string.IsNullOrEmpty(request?.projectUid))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("UpsertProjectSettings", request?.projectUid);
      log.LogDebug($"UpsertProjectSettings: {JsonConvert.SerializeObject(request)}");

      var projectSettingsRequest = requestFactory.Create<ProjectSettingsRequestHelper>(r => r
            .CustomerUid(customerUid))
          .CreateProjectSettingsRequest(request?.projectUid, request?.settings);
      projectSettingsRequest.Validate();

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            null, raptorProxy, null,
            projectRepo)
          .ProcessAsync(projectSettingsRequest)
      )) as ProjectSettingsResult;

      log.LogResult(this.ToString(), JsonConvert.SerializeObject(request), result);
      return result as ProjectSettingsResult;
    }
  }
}
