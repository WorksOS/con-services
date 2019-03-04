using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Project.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Push.Abstractions;
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

    private readonly INotificationHubClient notificationHubClient;


    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configStore"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="producer"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="requestFactory"></param>
    public ProjectSettingsV4Controller(ILoggerFactory logger, IConfigurationStore configStore, 
      IServiceExceptionHandler serviceExceptionHandler, IKafka producer,
      IRaptorProxy raptorProxy, ISubscriptionProxy subscriptionProxy,
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo,
      IRequestFactory requestFactory, INotificationHubClient notificationHubClient
      )
      : base(logger.CreateLogger<ProjectSettingsV4Controller>(), configStore, serviceExceptionHandler, 
          producer, raptorProxy, projectRepo)
    {
      this.logger = logger;
      this.requestFactory = requestFactory;
      this.notificationHubClient = notificationHubClient;
    }

    /// <summary>
    /// Gets the target project settings for a project and user.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    [Route("api/v4/projectsettings/{projectUid}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettingsTargets(string projectUid)
    {
      return await GetProjectSettingsForType(projectUid, ProjectSettingsType.Targets);
    }

    /// <summary>
    /// Gets the target settings for a project and user.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    [Route("api/v4/projectcolors/{projectUid}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettings(string projectUid)
    {
      return await GetProjectSettingsForType(projectUid, ProjectSettingsType.Colors);
    }


    /// <summary>
    /// Upserts the target project settings for a project and user.
    /// </summary>
    /// <returns></returns>
    [Route("api/v4/projectcolors")]
    [HttpPut]
    public async Task<ProjectSettingsResult> UpsertProjectColors([FromBody]ProjectSettingsRequest request)
    {
      if (string.IsNullOrEmpty(request?.projectUid))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("UpsertProjectSettings", request?.projectUid);
      log.LogDebug($"UpsertProjectSettings: {JsonConvert.SerializeObject(request)}");

      request.ProjectSettingsType = ProjectSettingsType.Colors;

      var projectSettingsRequest = requestFactory.Create<ProjectSettingsRequestHelper>(r => r
          .CustomerUid(customerUid))
        .CreateProjectSettingsRequest(request.projectUid, request.Settings, request.ProjectSettingsType);
      projectSettingsRequest.Validate();

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            raptorProxy, null, null, null, null,
            projectRepo)
          .ProcessAsync(projectSettingsRequest)
      )) as ProjectSettingsResult;

      await NotifyChanges(userId, request.projectUid);

      log.LogResult(this.ToString(), JsonConvert.SerializeObject(request), result);
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

      request.ProjectSettingsType = ProjectSettingsType.Targets;

      var projectSettingsRequest = requestFactory.Create<ProjectSettingsRequestHelper>(r => r
            .CustomerUid(customerUid))
          .CreateProjectSettingsRequest(request.projectUid, request.Settings, request.ProjectSettingsType);
      projectSettingsRequest.Validate();

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            raptorProxy, null, null, null, null,
            projectRepo)
          .ProcessAsync(projectSettingsRequest)
      )) as ProjectSettingsResult;

      await NotifyChanges(userId, request.projectUid);

      log.LogResult(this.ToString(), JsonConvert.SerializeObject(request), result);
      return result;
    }

    private async Task<ProjectSettingsResult> GetProjectSettingsForType(string projectUid, ProjectSettingsType settingsType)
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
            null, null, null, null, null,
            projectRepo)
          .ProcessAsync(projectSettingsRequest)
      )) as ProjectSettingsResult;

      log.LogResult(this.ToString(), projectUid, result);
      return result;
    }

    private Task NotifyChanges(string userUid, string projectUid)
    {
      // You'd like to think these are in the format of a guid, but a simple check will stop any cast exceptions

      var userTask = Guid.TryParse(userUid, out var u) 
        ? notificationHubClient.Notify(new UserChangedNotification(u)) 
        : Task.CompletedTask;

      var projectTask = Guid.TryParse(projectUid, out var p) 
        ? notificationHubClient.Notify(new ProjectChangedNotification(p)) 
        : Task.CompletedTask;

      return Task.WhenAll(userTask, projectTask);
    }
  }
}
