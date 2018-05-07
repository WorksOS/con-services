using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
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
    private readonly ILoggerFactory logger;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionsRepo"></param>
    /// <param name="store"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="fileRepo"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler">The ServiceException handler.</param>
    public ProjectV4Controller(IKafka producer, IProjectRepository projectRepo,
      ISubscriptionRepository subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subscriptionProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, IFileRepository fileRepo,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler)
      : base(producer, projectRepo, subscriptionsRepo, fileRepo, store, subscriptionProxy, geofenceProxy, raptorProxy,
        logger, serviceExceptionHandler, logger.CreateLogger<ProjectV4Controller>())
    {
      this.logger = logger;
    }

    #region projects

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types,
    /// except of Landfill projects, and both active and archived projects.
    /// </summary>
    /// <param name="includeLandfill">Determines whether to include or exclude Landfill projects.</param>
    /// <returns>A list of projects</returns>
    [Route("api/v4/project")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsListResult> GetProjectsV4([FromQuery] bool? includeLandfill)
    {
      log.LogInformation("GetProjectsV4");

      ImmutableList<Repositories.DBModels.Project> projects;

      if (!includeLandfill.HasValue || !includeLandfill.Value)
      {
        //exclude Landfill Projects for now
        projects = (await GetProjectList().ConfigureAwait(false))
          .Where(prj => prj.ProjectType != ProjectType.LandFill).ToImmutableList();
      }
      else
      {
        projects = (await GetProjectList().ConfigureAwait(false)).ToImmutableList();
      }

      return new ProjectV4DescriptorsListResult
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(project))
          .ToImmutableList()
      };
    }

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    /// and both active and archived projects.
    /// </summary>
    /// <returns>A list of projects</returns>
    [Route("api/v4/project/all")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsListResult> GetAllProjectsV4()
    {
      log.LogInformation("GetAllProjectsV4");

      //exclude Landfill Projects for now
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
      var project = await GetProject(projectUid).ConfigureAwait(false);
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

      //Landfill projects are not supported till l&s goes live
      if (projectRequest?.ProjectType == ProjectType.LandFill)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 73);
      
      log.LogInformation("CreateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      if (projectRequest.CustomerUID == null) projectRequest.CustomerUID = Guid.Parse(customerUid);
      if (projectRequest.ProjectUID == null) projectRequest.ProjectUID = Guid.NewGuid();

      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(projectRequest);
      createProjectEvent.ReceivedUTC = createProjectEvent.ActionUTC = DateTime.UtcNow;
      ProjectDataValidator.Validate(createProjectEvent, projectRepo);
      if (createProjectEvent.CustomerUID.ToString() != customerUid)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 18);
      }

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            geofenceProxy, raptorProxy, subscriptionProxy,
            projectRepo, subscriptionRepo, fileRepo)
          .ProcessAsync(createProjectEvent)
      );

      var result = new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await GetProject(createProjectEvent.ProjectUID.ToString())
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

      //Landfill projects are not supported till l&s goes live
      if (projectRequest?.ProjectType == ProjectType.LandFill)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 73);

      log.LogInformation("UpdateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;

      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, projectRepo);
      
      await ProjectRequestHelper.ValidateCoordSystemInRaptor(project,
        serviceExceptionHandler, customHeaders, raptorProxy).ConfigureAwait(false);

      /*** now making changes, potentially needing rollback ***/
      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        var projectWithLegacyProjectId = projectRepo.GetProjectOnly(project.ProjectUID.ToString()).Result;
        await ProjectRequestHelper.CreateCoordSystemInRaptor(project.ProjectUID, projectWithLegacyProjectId.LegacyProjectID,
          project.CoordinateSystemFileName, project.CoordinateSystemFileContent, false,
          log, serviceExceptionHandler, customerUid, customHeaders,
          projectRepo, raptorProxy).ConfigureAwait(false);
      }

      var isUpdated = await projectRepo.StoreEvent(project).ConfigureAwait(false);
      if (isUpdated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 62);

      var messagePayload = JsonConvert.SerializeObject(new { UpdateProjectEvent = project });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });

      log.LogInformation("UpdateProjectV4. Completed successfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await GetProject(project.ProjectUID.ToString())
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
    public async Task<ProjectV4DescriptorsSingleResult> DeleteProjectV4([FromUri] string projectUid)
    {
      LogCustomerDetails("DeleteProjectV4", projectUid);
      var project = new DeleteProjectEvent
      {
        ProjectUID = Guid.Parse(projectUid),
        DeletePermanently = false,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      ProjectDataValidator.Validate(project, projectRepo);

      var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
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
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await GetProject(project.ProjectUID.ToString())
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


  }
}