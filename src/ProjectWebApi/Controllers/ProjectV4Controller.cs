using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v4
  /// </summary>
  public class ProjectV4Controller : ProjectBaseController
  {
    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory _logger;

    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor HttpContextAccessor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="store"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="fileRepo"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler">The ServiceException handler.</param>
    /// <param name="httpContextAccessor"></param>
    public ProjectV4Controller(IKafka producer, IProjectRepository projectRepo,
      ISubscriptionRepository subscriptionRepo, IFileRepository fileRepo,
      IConfigurationStore store,
      ISubscriptionProxy subscriptionProxy, IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IHttpContextAccessor httpContextAccessor)
      : base(producer, projectRepo, subscriptionRepo, fileRepo, store, subscriptionProxy, geofenceProxy, raptorProxy,
        logger, serviceExceptionHandler, logger.CreateLogger<ProjectV4Controller>())
    {
      this._logger = logger;
      this.HttpContextAccessor = httpContextAccessor;
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
      log.LogInformation("GetAllProjectsV4");

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
      log.LogInformation("GetProjectV4");

      var project = await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, log, serviceExceptionHandler, projectRepo).ConfigureAwait(false);
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
    [Route("api/v4/project")]
    [HttpPost]
    public async Task<ProjectV4DescriptorsSingleResult> CreateProjectV4([FromBody] CreateProjectRequest projectRequest)
    {
      if (projectRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);
      }

      log.LogInformation("CreateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      if (projectRequest.CustomerUID == null) projectRequest.CustomerUID = Guid.Parse(customerUid);
      if (projectRequest.ProjectUID == null) projectRequest.ProjectUID = Guid.NewGuid();

      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(projectRequest);
      createProjectEvent.ReceivedUTC = createProjectEvent.ActionUTC = DateTime.UtcNow;
      ProjectDataValidator.Validate(createProjectEvent, projectRepo, serviceExceptionHandler);
      if (createProjectEvent.CustomerUID.ToString() != customerUid)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 18);
      }
      await ProjectDataValidator.ValidateProjectName(customerUid, createProjectEvent.ProjectName, createProjectEvent.ProjectUID.ToString(), log, serviceExceptionHandler, projectRepo);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(_logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            geofenceProxy, raptorProxy, subscriptionProxy,
            projectRepo, subscriptionRepo, fileRepo, null, HttpContextAccessor)
          .ProcessAsync(createProjectEvent)
      );

      var result = new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await ProjectRequestHelper.GetProject(createProjectEvent.ProjectUID.ToString(), customerUid, log, serviceExceptionHandler, projectRepo)
          .ConfigureAwait(false)));

      log.LogResult(this.ToString(), JsonConvert.SerializeObject(projectRequest), result);
      return result;
    }

    // PUT: api/v4/project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="projectRequest">UpdateProjectRequest model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project")]
    [HttpPut]
    public async Task<ProjectV4DescriptorsSingleResult> UpdateProjectV4([FromBody] UpdateProjectRequest projectRequest)
    {
      if (projectRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 40);
      }

      log.LogInformation("UpdateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;

      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, projectRepo, serviceExceptionHandler);
      await ProjectDataValidator.ValidateProjectName(customerUid, projectRequest.ProjectName, projectRequest.ProjectUid.ToString(), log, serviceExceptionHandler, projectRepo);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdateProjectExecutor>(_logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            geofenceProxy, raptorProxy, subscriptionProxy,
            projectRepo, subscriptionRepo, fileRepo, null, HttpContextAccessor)
          .ProcessAsync(project)
      );

      log.LogInformation("UpdateProjectV4. Completed successfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID.ToString(), customerUid, log, serviceExceptionHandler, projectRepo)
          .ConfigureAwait(false)));
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
      ProjectDataValidator.Validate(project, projectRepo, serviceExceptionHandler);

      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
      var isDeleted = await projectRepo.StoreEvent(project).ConfigureAwait(false);
      if (isDeleted == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });

      log.LogInformation("DeleteProjectV4. Completed succesfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID.ToString(), customerUid, log, serviceExceptionHandler, projectRepo)
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
      log.LogInformation("GetAssociatedGeofencesV4");

      if (projectUid != null)
      {
        var project = await ProjectRequestHelper
          .GetProject(projectUid.ToString(), customerUid, log, serviceExceptionHandler, projectRepo).ConfigureAwait(false);
        if (project.ProjectType != ProjectType.LandFill)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 102);
        }
        ProjectDataValidator.ValidateGeofenceTypes(geofenceType, project.ProjectType);
      }
      else
      {
        ProjectDataValidator.ValidateGeofenceTypes(geofenceType);
      }

      var geofences =
        (await ProjectRequestHelper.GetGeofenceList(customerUid, projectUid != null ? projectUid.ToString() : string.Empty, geofenceType, log, projectRepo));
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
      log.LogInformation("UpdateProjectGeofencesV4");

      updateProjectGeofenceRequest.Validate();
      log.LogInformation($"UpdateProjectGeofencesV4 validation passed: {updateProjectGeofenceRequest}");

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdateProjectGeofenceExecutor>(_logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            geofenceProxy, raptorProxy, subscriptionProxy,
            projectRepo, subscriptionRepo, fileRepo, null, HttpContextAccessor)
          .ProcessAsync(updateProjectGeofenceRequest)
      );

      log.LogInformation("UpdateProjectGeofencesV4. Completed successfully");
      return new ContractExecutionResult();
    }

    #endregion geofences

  }
}