using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v4
  /// </summary>
  public class ProjectV4Controller : ProjectBaseController
  {
    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor HttpContextAccessor;

    private readonly INotificationHubClient notificationHubClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV4Controller(IKafka producer, IConfigurationStore configStore, ISubscriptionProxy subscriptionProxy, IHttpContextAccessor httpContextAccessor, INotificationHubClient notificationHubClient)
      : base(producer, configStore, subscriptionProxy)
    {
      this.HttpContextAccessor = httpContextAccessor;
      this.notificationHubClient = notificationHubClient;
    }

    #region projects

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types,
    ///    and both active and archived projects.
    /// </summary>
    /// <param name="includeLandfill">Obsolete</param>
    /// <returns>A list of projects</returns>
    [Route("api/v4/project")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsListResult> GetProjectsV4([FromQuery] bool? includeLandfill)
    {
      // Note: includeLandfill is obsolete, but not worth the grief up creating a new endpoint.

      return await GetAllProjectsV4();
    }

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    ///        and both active and archived projects.
    /// </summary>
    /// <returns>A list of projects</returns>
    [Route("api/v4/project/all")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsListResult> GetAllProjectsV4()
    {
      Logger.LogInformation("GetAllProjectsV4");

      var projects = (await GetProjectList().ConfigureAwait(false)).ToImmutableList();

      return new ProjectV4DescriptorsListResult
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(project))
          .ToImmutableList()
      };
    }

    /// <summary>
    /// Gets a project for a customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("api/v4/project/{projectUid}")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsSingleResult> GetProjectV4(string projectUid)
    {
      Logger.LogInformation("GetProjectV4");
     
      var project = await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
      return new ProjectV4DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(project));
    }

    // POST: api/project
    /// <summary>
    /// Create Project
    ///    as of v4 this creates a project AND the association to Customer
    /// </summary>
    /// <param name="projectRequest">CreateProjectRequest model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("internal/v4/project")]
    [Route("api/v4/project")]
    [HttpPost]
    public async Task<ProjectV4DescriptorsSingleResult> CreateProjectV4([FromBody] CreateProjectRequest projectRequest)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);
      }

      Logger.LogInformation("CreateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      if (projectRequest.CustomerUID == null) projectRequest.CustomerUID = Guid.Parse(customerUid);
      if (projectRequest.ProjectUID == null) projectRequest.ProjectUID = Guid.NewGuid();

      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(projectRequest);
      createProjectEvent.ReceivedUTC = createProjectEvent.ActionUTC = DateTime.UtcNow;
      ProjectDataValidator.Validate(createProjectEvent, ProjectRepo, ServiceExceptionHandler);
      if (createProjectEvent.CustomerUID.ToString() != customerUid)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 18);
      }
      await ProjectDataValidator.ValidateProjectName(customerUid, createProjectEvent.ProjectName, createProjectEvent.ProjectUID.ToString(), Logger, ServiceExceptionHandler, ProjectRepo);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, customHeaders, Producer, KafkaTopicName,
            productivity3dV1ProxyCoord: Productivity3dV1ProxyCoord,
            subscriptionProxy: subscriptionProxy, projectRepo: ProjectRepo, subscriptionRepo: SubscriptionRepo, fileRepo: FileRepo,
            httpContextAccessor: HttpContextAccessor, dataOceanClient: DataOceanClient, authn: Authorization)
          .ProcessAsync(createProjectEvent)
      );

      var result = new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await ProjectRequestHelper.GetProject(createProjectEvent.ProjectUID.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo)
          .ConfigureAwait(false)));

      await notificationHubClient.Notify(new CustomerChangedNotification(projectRequest.CustomerUID.Value));

      Logger.LogResult(this.ToString(), JsonConvert.SerializeObject(projectRequest), result);
      return result;
    }

    /// <summary>
    /// Create a scheduler job to create a project using internal urls 
    /// </summary>
    /// <param name="projectRequest">The project request model to be used</param>
    /// <param name="scheduler">The scheduler used to queue the job</param>
    /// <returns>Scheduler Job Result, containing the Job ID To poll via the Scheduler</returns>
    [Route("api/v4/project/background")]
    [HttpPost]
    public async Task<ScheduleJobResult> RequestCreateProjectBackgroundJob([FromBody] CreateProjectRequest projectRequest, [FromServices] ISchedulerProxy scheduler)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);
      }
      
      var baseUrl = Request.Host.ToUriComponent(); 
      var callbackUrl = $"http://{baseUrl}/internal/v4/project";  
      Logger.LogInformation($"nameof(RequestCreateProjectBackgroundJob): baseUrl {callbackUrl}");

      var request = new ScheduleJobRequest
      {
        Filename = projectRequest.ProjectName + Guid.NewGuid(), // Make sure the filename is unique, it's not important what it's called as the scheduled job keeps a reference
        Method = "POST",
        Url = callbackUrl,
        Headers =
        {
          ["Content-Type"] = Request.Headers["Content-Type"]
        }
      };
      request.SetBinaryPayload(Request.Body);

      var customHeaders = Request.Headers.GetCustomHeaders();

      return await scheduler.ScheduleBackgroundJob(request, customHeaders);
    }

    // PUT: api/v4/project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="projectRequest">UpdateProjectRequest model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("internal/v4/project")]
    [Route("api/v4/project")]
    [HttpPut]
    public async Task<ProjectV4DescriptorsSingleResult> UpdateProjectV4([FromBody] UpdateProjectRequest projectRequest)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 40);
      }

      Logger.LogInformation("UpdateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;

      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, ProjectRepo, ServiceExceptionHandler);
      await ProjectDataValidator.ValidateProjectName(customerUid, projectRequest.ProjectName, projectRequest.ProjectUid.ToString(), Logger, ServiceExceptionHandler, ProjectRepo);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, customHeaders,
            Producer, KafkaTopicName,
            productivity3dV1ProxyCoord: Productivity3dV1ProxyCoord,
            subscriptionProxy: subscriptionProxy, projectRepo: ProjectRepo, subscriptionRepo: SubscriptionRepo, fileRepo: FileRepo, httpContextAccessor: HttpContextAccessor,
            dataOceanClient: DataOceanClient, authn: Authorization)
          .ProcessAsync(project)
      );

      //invalidate cache in TRex/Raptor
      Logger.LogInformation("UpdateProjectV4. Invalidating 3D PM cache");
      await notificationHubClient.Notify(new ProjectChangedNotification(project.ProjectUID));

      Logger.LogInformation("UpdateProjectV4. Completed successfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo)
          .ConfigureAwait(false)));
    }

    /// <summary>
    /// Create a scheduler job to update an existing project in the background
    /// </summary>
    /// <param name="projectRequest">The project request model to be used in the update</param>
    /// <param name="scheduler">The scheduler used to queue the job</param>
    /// <returns>Scheduler Job Result, containing the Job ID To poll via the Scheduler</returns>
    [Route("api/v4/project/background")]
    [HttpPut]
    public async Task<ScheduleJobResult> RequestUpdateProjectBackgroundJob([FromBody] UpdateProjectRequest projectRequest, [FromServices] ISchedulerProxy scheduler)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);
      }

      // do a quick validation to make sure the project acctually exists (this will also be run in the background task, but a quick response to the UI will be better if the project can't be updated)
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;
      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, ProjectRepo, ServiceExceptionHandler);
      await ProjectDataValidator.ValidateProjectName(customerUid, projectRequest.ProjectName, projectRequest.ProjectUid.ToString(), Logger, ServiceExceptionHandler, ProjectRepo);

      var baseUrl = Request.Host.ToUriComponent();
      var callbackUrl = $"http://{baseUrl}/internal/v4/project";
      Logger.LogInformation($"nameof(RequestUpdateProjectBackgroundJob): baseUrl {callbackUrl}");

      var request = new ScheduleJobRequest
      {
        Filename = projectRequest.ProjectName + Guid.NewGuid(), // Make sure the filename is unique, it's not important what it's called as the scheduled job keeps a reference
        Method = "PUT",
        Url = callbackUrl,
        Headers =
        {
          ["Content-Type"] = Request.Headers["Content-Type"]
        }
      };
      request.SetBinaryPayload(Request.Body);

      var customHeaders = Request.Headers.GetCustomHeaders();

      return await scheduler.ScheduleBackgroundJob(request, customHeaders);
    }

    // DELETE: api/Project/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="projectUid">projectUid to delete</param>
    /// <remarks>Deletes existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project/{projectUid}")]
    [HttpDelete]
    public async Task<ProjectV4DescriptorsSingleResult> DeleteProjectV4([FromRoute] string projectUid)
    {
      LogCustomerDetails("DeleteProjectV4", projectUid);
      var project = new DeleteProjectEvent
      {
        ProjectUID = Guid.Parse(projectUid),
        DeletePermanently = false,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      ProjectDataValidator.Validate(project, ProjectRepo, ServiceExceptionHandler);

      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
      var isDeleted = await ProjectRepo.StoreEvent(project).ConfigureAwait(false);
      if (isDeleted == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

      Producer.Send(KafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });

      if (Guid.TryParse(customerUid, out var c))
        await notificationHubClient.Notify(new CustomerChangedNotification(c));

      Logger.LogInformation("DeleteProjectV4. Completed succesfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo)
          .ConfigureAwait(false)));

    }

    #endregion projects


    #region subscriptions

    /// <summary>
    /// Gets available subscription for a customer
    /// </summary>
    /// <returns>List of available subscriptions</returns>
    [Route("api/v4/subscriptions")]
    [HttpGet]
    public async Task<SubscriptionsListResult> GetSubscriptionsV4()
    {
      var customerUid = LogCustomerDetails("GetSubscriptionsV4", "");

      //returns empty list if no subscriptions available
      return new SubscriptionsListResult
      {
        SubscriptionDescriptors =
          (await GetFreeSubs(customerUid).ConfigureAwait(false)).Select(
            SubscriptionDescriptor.FromSubscription).ToImmutableList()
      };
    }

    #endregion subscriptions


    #region geofences

    /// <summary>
    /// Note that only Landfill Projects and Landfill sites are supported.
    ///
    /// If projectUid is NOT provided,
    ///    Returns a list of geofences for a customer, which are NOT associated with any project.
    ///     The list includes projects of selected types.
    ///     Includes only !deleted Geofences.
    ///
    /// If projectUid IS provided,
    ///   Gets a list of geofences associated with particular project.
    ///     The list includes geofences of selected types.
    ///     Includes only !deleted Geofences.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A list of geofences</returns>
    [Route("api/v4/geofences")]
    [HttpGet]
    public async Task<GeofenceV4DescriptorsListResult> GetAssociatedGeofencesV4([FromQuery] List<GeofenceType> geofenceType, [FromQuery] Guid? projectUid = null)
    {
      Logger.LogInformation("GetAssociatedGeofencesV4");

      if (projectUid != null)
      {
        var project = await ProjectRequestHelper
          .GetProject(projectUid.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
        if (project.ProjectType != ProjectType.LandFill)
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 102);
        }
        ProjectDataValidator.ValidateGeofenceTypes(geofenceType, project.ProjectType);
      }
      else
      {
        ProjectDataValidator.ValidateGeofenceTypes(geofenceType);
      }

      var geofences =
        (await ProjectRequestHelper.GetGeofenceList(customerUid, projectUid != null ? projectUid.ToString() : string.Empty, geofenceType, Logger, ProjectRepo));
      return new GeofenceV4DescriptorsListResult
      {
        GeofenceDescriptors = geofences.Select(geofence =>
            AutoMapperUtility.Automapper.Map<GeofenceV4Descriptor>(geofence))
          .ToImmutableList()
      };
    }

    /// <summary>
    /// Update ProjectGeofence Associations for selected geofenceType
    /// </summary>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/geofences")]
    [HttpPut]
    public async Task<ContractExecutionResult> UpdateProjectGeofencesV4(
      [FromBody] UpdateProjectGeofenceRequest updateProjectGeofenceRequest)
    {
      Logger.LogInformation("UpdateProjectGeofencesV4");

      updateProjectGeofenceRequest.Validate();
      Logger.LogInformation($"UpdateProjectGeofencesV4 validation passed: {updateProjectGeofenceRequest}");

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdateProjectGeofenceExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, customHeaders,
            Producer, KafkaTopicName,
            subscriptionProxy: subscriptionProxy, projectRepo: ProjectRepo, subscriptionRepo: SubscriptionRepo, fileRepo: FileRepo, httpContextAccessor: HttpContextAccessor)
          .ProcessAsync(updateProjectGeofenceRequest)
      );

      Logger.LogInformation("UpdateProjectGeofencesV4. Completed successfully");
      return new ContractExecutionResult();
    }

    #endregion geofences
  }
}
