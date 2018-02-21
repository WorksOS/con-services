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
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Settings controller, version 4.
  /// </summary>
  public class ProjectSettingsV4Controller : BaseController
  {
    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IRequestFactory requestFactory;


    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configStore"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="producer"></param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionsRepo"></param>
    /// <param name="requestFactory"></param>
    public ProjectSettingsV4Controller(ILoggerFactory logger, IConfigurationStore configStore, 
      IServiceExceptionHandler serviceExceptionHandler, IKafka producer,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ISubscriptionProxy subscriptionProxy,
      IRepository<IProjectEvent> projectRepo, IRepository<ISubscriptionEvent> subscriptionsRepo,
      IRequestFactory requestFactory
      )
      : base(logger.CreateLogger<ProjectSettingsV4Controller>(), configStore, serviceExceptionHandler, 
          producer, raptorProxy, projectRepo)
    {
      this.logger = logger;
      this.requestFactory = requestFactory;
    }


    /// <summary>
    /// Gets the target project settings for a project and user.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="settingsType">The project settings' type.</param>
    /// <returns></returns>
    [Route("api/v4/projectsettings/{projectUid}/{settingsType}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettings(string projectUid, ProjectSettingsType settingsType)
    {
      if (string.IsNullOrEmpty(projectUid))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("GetProjectSettings", projectUid);

      var projectSettingsRequest = requestFactory.Create<ProjectSettingsRequestHelper>(r => r
          .CustomerUid(customerUid))
        .CreateProjectSettingsRequest(projectUid, string.Empty, settingsType);
      projectSettingsRequest.Validate();
     
      var result = (await WithServiceExceptionTryExecuteAsync(() => 
        RequestExecutorContainerFactory
        .Build<GetProjectSettingsExecutor>(logger, configStore, serviceExceptionHandler, 
                customerUid, userId, null, null,
                null, null,
                null, null, null,
                projectRepo)
        .ProcessAsync(projectSettingsRequest) 
        )) as ProjectSettingsResult;
     
      log.LogResult(this.ToString(), projectUid, result);
      return result;
    }


    /// <summary>
    /// Upserts the target project settings for a project and user.
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
          .CreateProjectSettingsRequest(request?.projectUid, request?.Settings, ProjectSettingsType.Targets);
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
      return result;
    }
  }
}
