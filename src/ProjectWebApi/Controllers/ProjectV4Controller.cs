using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using MasterDataProxies.ResultHandling;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Filters;
using ProjectWebApi.Internal;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using ProjectWebApiCommon.Utilities;
using Repositories;
using Repositories.DBModels;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ContractExecutionResult = ProjectWebApiCommon.ResultsHandling.ContractExecutionResult;

namespace Controllers
{
  /// <summary>
  /// Project controller v4
  /// </summary>
  public class ProjectV4Controller : ProjectBaseController
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionsRepo"></param>
    /// <param name="store"></param>
    /// <param name="subsProxy"></param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler">The ServiceException handler.</param>
    public ProjectV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
      : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, raptorProxy, logger, serviceExceptionHandler)
    {
    }

    #region projects

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    /// and both active and archived projects.
    /// </summary>
    /// <returns>A list of projects</returns>
    [Route("api/v4/project")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsListResult> GetProjectsV4()
    {
      log.LogInformation("GetProjectsV4");
      //exclude Landfill Projects for now
      //exclude Landfill Projects for now
      var projects = (await GetProjectList().ConfigureAwait(false)).Where(prj => prj.ProjectType != ProjectType.LandFill).ToImmutableList();

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

      var customerUid = (User as TIDCustomPrincipal).CustomerUid;
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);
      }

      //Landfill projects are not supported till l&s goes live
      if (projectRequest?.ProjectType == ProjectType.LandFill)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(3000, "Landfill projects are not supported"));


      log.LogInformation("CreateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      if (projectRequest.CustomerUID == null) projectRequest.CustomerUID = Guid.Parse(customerUid);
      if (projectRequest.ProjectUID == null) projectRequest.ProjectUID = Guid.NewGuid();

      var project = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;
      ProjectDataValidator.Validate(project, projectService);
      if (project.CustomerUID.ToString() != customerUid)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 18);
      }

      await ValidateCoordSystemInRaptor(project).ConfigureAwait(false);

      log.LogDebug($"Testing if there are overlapping projects for project {project.ProjectName}");
      await DoesProjectOverlap(project, project.ProjectBoundary);

      AssociateProjectCustomer customerProject = new AssociateProjectCustomer
      {
        CustomerUID = project.CustomerUID,
        LegacyCustomerID = project.CustomerID,
        ProjectUID = project.ProjectUID,
        RelationType = RelationType.Owner,
        ActionUTC = project.ActionUTC,
        ReceivedUTC = project.ReceivedUTC
      };
      ProjectDataValidator.Validate(customerProject, projectService);


      /*** now making changes, potentially needing rollback ***/
      project = await CreateProjectInDb(project, customerProject).ConfigureAwait(false);
      await CreateCoordSystemInRaptor(project.ProjectUID, project.ProjectID, project.CoordinateSystemFileName, project.CoordinateSystemFileContent, true).ConfigureAwait(false);
      await AssociateProjectSubscriptionInSubscriptionService(project).ConfigureAwait(false);
      await CreateGeofenceInGeofenceService(project).ConfigureAwait(false);

      // doing this as late as possible in case something fails. We can't cleanup kafka que.
      CreateKafkaEvents(project, customerProject);

      log.LogDebug("CreateProjectV4. completed succesfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await GetProject(project.ProjectUID.ToString())
          .ConfigureAwait(false)));
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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 40);
      }

      //Landfill projects are not supported till l&s goes live
      if (projectRequest?.ProjectType == ProjectType.LandFill)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(3000, "Landfill projects are not supported"));

      log.LogInformation("UpdateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;

      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, projectService);
      await ValidateCoordSystemInRaptor(project).ConfigureAwait(false);

      
      /*** now making changes, potentially needing rollback ***/
      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        var projectWithLegacyProjectId = projectService.GetProjectOnly(project.ProjectUID.ToString()).Result;
        await CreateCoordSystemInRaptor(project.ProjectUID, projectWithLegacyProjectId.LegacyProjectID,
          project.CoordinateSystemFileName, project.CoordinateSystemFileContent, false).ConfigureAwait(false);
      }

      var isUpdated = await projectService.StoreEvent(project).ConfigureAwait(false);
      if (isUpdated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 62);

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
      ProjectDataValidator.Validate(project, projectService);

      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
      var isDeleted = await projectService.StoreEvent(project).ConfigureAwait(false);
      if (isDeleted == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

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
      var customerUid = LogCustomerDetails("GetSubscriptionsV4");

      //returns empty list if no subscriptions available
      return new SubscriptionsListResult
      {
        SubscriptionDescriptors =
          (await GetFreeSubs(customerUid).ConfigureAwait(false)).Select(
            SubscriptionDescriptor.FromSubscription).ToImmutableList()
      };
    }

    #endregion subscriptions


    #region privateValidation

    /// <summary>
    /// Determines if the project boundary overlaps any exising project for the customer in time and space.
    ///    not needed for v1 or 3 and they come via CGen which already does this overlap checking.
    /// </summary>    
    /// <param name="project">The create project event</param>
    /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
    /// <returns></returns>
    private async Task<bool> DoesProjectOverlap(CreateProjectEvent project, string databaseProjectBoundary)
    {
      var overlaps =
        await projectService.DoesPolygonOverlap(project.CustomerUID.ToString(), databaseProjectBoundary,
          project.ProjectStartDate, project.ProjectEndDate).ConfigureAwait(false);
      if (overlaps)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 43);

      log.LogDebug($"No overlapping projects for {project.ProjectName}");
      return overlaps;
    }

    /// <summary>
    /// validate CordinateSystem if provided
    /// </summary>
    /// <param name=""></param>
    private async Task<bool> ValidateCoordSystemInRaptor(IProjectEvent project)
    {
      // a Creating a landfill must have a CS, else optional
      //  if updating a landfill, or other then May have one. Note that a null one doesn't overwrite any existing.
      if (project is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)project;
        if (projectEvent.ProjectType == ProjectType.LandFill
            && (string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName)
                || projectEvent.CoordinateSystemFileContent == null)
        )
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 45);
      }

      if (project is CreateProjectEvent)
        ProjectBoundaryValidator.ValidateWKT(((CreateProjectEvent)project).ProjectBoundary);

      var csFileName = (project is CreateProjectEvent)
        ? ((CreateProjectEvent)project).CoordinateSystemFileName
        : ((UpdateProjectEvent)project).CoordinateSystemFileName;
      var csFileContent = (project is CreateProjectEvent)
        ? ((CreateProjectEvent)project).CoordinateSystemFileContent
        : ((UpdateProjectEvent)project).CoordinateSystemFileContent;
      if (!string.IsNullOrEmpty(csFileName) || csFileContent != null)
      {
        ProjectDataValidator.ValidateFileName(csFileName);
        CoordinateSystemSettingsResult coordinateSystemSettingsResult = null;
        try
        {
          coordinateSystemSettingsResult = await raptorProxy
            .CoordinateSystemValidate(csFileContent, csFileName, Request.Headers.GetCustomHeaders())
            .ConfigureAwait(false);
        }
        catch (Exception e)
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.CoordinateSystemValidate", e.Message);
        }
        if (coordinateSystemSettingsResult == null)
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 46);

        if (coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 47, coordinateSystemSettingsResult.Code.ToString(),
            coordinateSystemSettingsResult.Message);
        }
      }
      return true;
    }

    #endregion privateValidation


    #region private

    /// <summary>
    /// Creates a project. Handles both old and new project boundary formats.
    /// </summary>
    /// <param name="project">The create project event</param>
    /// <param name="customerProject"></param>
    /// <returns></returns>
    private async Task<CreateProjectEvent> CreateProjectInDb(CreateProjectEvent project, AssociateProjectCustomer customerProject)
    {
      log.LogDebug($"Creating the project in the DB {JsonConvert.SerializeObject(project)} and customerProject {JsonConvert.SerializeObject(customerProject)}");

      var isCreated = await projectService.StoreEvent(project).ConfigureAwait(false);
      if (isCreated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 61);

      log.LogDebug($"Created the project in DB. IsCreated: {isCreated}. projectUid: {project.ProjectUID} legacyprojectID: {project.ProjectID}");

      if (project.ProjectID <= 0)
      {
        var existing = await projectService.GetProjectOnly(project.ProjectUID.ToString()).ConfigureAwait(false);
        if (existing != null && existing.LegacyProjectID > 0)
          project.ProjectID = existing.LegacyProjectID;
        else
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 42);
        }
      }
      log.LogDebug($"Using Legacy projectId {project.ProjectID} for project {project.ProjectName}");

      // this is needed so that when ASNode (raptor client), which is called from CoordinateSystemPost, can retrieve the just written project+cp
      isCreated = await projectService.StoreEvent(customerProject).ConfigureAwait(false);
      if (isCreated == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 63);
 
      log.LogDebug($"Created CustomerProject in DB {customerProject}");
      return project; // legacyID may have been added
    }

    private async Task CreateCoordSystemInRaptor(Guid projectUid, int legacyProjectId, string coordinateSystemFileName, byte[] coordinateSystemFileContent, bool isCreate)
    {
      if (!string.IsNullOrEmpty(coordinateSystemFileName))
      {
        var customHeaders = Request.Headers.GetCustomHeaders();
        customHeaders.TryGetValue("X-VisionLink-ClearCache", out string caching);
        if (string.IsNullOrEmpty(caching)) // may already have been set by acceptance tests
          customHeaders.Add("X-VisionLink-ClearCache", "true");

        try
        {
          var coordinateSystemSettingsResult = await raptorProxy
            .CoordinateSystemPost(legacyProjectId, coordinateSystemFileContent,
              coordinateSystemFileName, customHeaders).ConfigureAwait(false);
          var message = string.Format($"Post of CS create to RaptorServices returned code: {0} Message {1}.",
            coordinateSystemSettingsResult?.Code ?? -1,
            coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
          log.LogDebug(message);
          if (coordinateSystemSettingsResult == null ||
              coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
          {
            if (isCreate)
              await DeleteProjectPermanentlyInDb(Guid.Parse((User as TIDCustomPrincipal).CustomerUid), projectUid).ConfigureAwait(false);

            ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 41, (coordinateSystemSettingsResult?.Code ?? -1).ToString(), (coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null"));
          }
        }
        catch (Exception e)
        {
          if (isCreate)
            await DeleteProjectPermanentlyInDb(Guid.Parse((User as TIDCustomPrincipal).CustomerUid), projectUid).ConfigureAwait(false);

          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.CoordinateSystemPost", e.Message);
        }
      }
    }
    
    /// <summary>
    /// Used internally, if a step fails, after a project has been CREATED, 
    ///    then delete it permanently i.e. don't just set IsDeleted.
    /// Since v4 CreateProjectInDB also associates projectCustomer then roll this back also.
    /// DissociateProjectCustomer actually deletes the DB ent4ry
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    private async Task DeleteProjectPermanentlyInDb(Guid customerUid, Guid projectUid)
    {
      log.LogDebug($"DeleteProjectPermanentlyInDB: {projectUid}");
      var deleteProjectEvent = new DeleteProjectEvent()
      {
        ProjectUID = projectUid,
        DeletePermanently = true,
        ActionUTC = DateTime.UtcNow
      };
      await projectService.StoreEvent(deleteProjectEvent).ConfigureAwait(false);

      await projectService.StoreEvent(new DissociateProjectCustomer()
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ActionUTC = DateTime.UtcNow
      }).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Creates Kafka events.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="customerProject">The create projectCustomer event</param>
    /// <returns></returns>
    private void CreateKafkaEvents(CreateProjectEvent project, AssociateProjectCustomer customerProject)
    {
      log.LogDebug($"CreateProjectEvent on kafka queue {JsonConvert.SerializeObject(project)}");
      string wktBoundary = project.ProjectBoundary;

      // Convert to old format for Kafka for consistency on kakfa queue
      string kafkaBoundary = project.ProjectBoundary
        .Replace(ProjectBoundaryValidator.POLYGON_WKT, string.Empty)
        .Replace("))", string.Empty)
        .Replace(',', ';')
        .Replace(' ', ',');

      var messagePayloadProject = JsonConvert.SerializeObject(new { CreateProjectEvent = project });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayloadProject)
        });
      //Save boundary as WKT
      project.ProjectBoundary = wktBoundary;

      log.LogDebug(
        $"AssociateCustomerProjectEvent on kafka queue {customerProject.ProjectUID} with Customer {customerProject.CustomerUID}");
      var messagePayloadCustomerProject = JsonConvert.SerializeObject(new { AssociateProjectCustomer = customerProject });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayloadCustomerProject)
        });
    }
    
    /// <summary>
    /// Gets the free subs for a project type
    /// </summary>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    /// <exception cref="ContractExecutionResult">No available subscriptions for the selected customer</exception>
    private async Task<ImmutableList<Subscription>> GetFreeSubs(string customerUid, ProjectType type, Guid projectUid)
    {
      var availableFreSub =
        (await subsService.GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date).ConfigureAwait(false))
        .Where(s => s.ServiceTypeID == (int)type.MatchSubscriptionType()).ToImmutableList();
      
      log.LogDebug($"We have {availableFreSub.Count} free subscriptions for the selected project type {type.ToString()}");
      if (!availableFreSub.Any())
      {
        await DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid).ConfigureAwait(false);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 37);
      }
      return availableFreSub;
    }

    /// <summary>
    /// Gets the free subscription regardless project type.
    /// </summary>
    /// <param name="customerUid">The customer uid.</param>
    /// <returns></returns>
    private async Task<ImmutableList<Subscription>> GetFreeSubs(string customerUid)
    {
      return
      (await subsService.GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date)
        .ConfigureAwait(false)).ToImmutableList();
    }

    /// <summary>
    /// Validates if there any subscriptions available for the request create project event
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    private async Task AssociateProjectSubscriptionInSubscriptionService(CreateProjectEvent project)
    {
      var customerUid = LogCustomerDetails("AssociateProjectSubscriptionInSubscriptionService", project.ProjectUID.ToString());

      if (project.ProjectType == ProjectType.LandFill || project.ProjectType == ProjectType.ProjectMonitoring)
      {
        subscriptionUidAssigned = Guid.Parse((await GetFreeSubs(customerUid, project.ProjectType, project.ProjectUID)).First().SubscriptionUID);
        log.LogDebug($"Received {subscriptionUidAssigned} subscription");
        //Assign a new project to a subscription
        try
        {
          // rethrows any exception
          await subsProxy.AssociateProjectSubscription(subscriptionUidAssigned,
            project.ProjectUID, Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          // this is only called from a Create, so no need to consider Update
          await DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID).ConfigureAwait(false);

          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "subsProxy.AssociateProjectSubscriptionInSubscriptionService", e.Message);
        }
      }
    }

    /// <summary>
    /// rolls back the ProjectSubscription association made, due to a subsequent error
    /// </summary>
    /// <returns></returns>
    private async Task DissociateProjectSubscription(Guid projectUid, Guid subscriptionUidAssigned)
    {
      var customerUid = LogCustomerDetails("DissociateProjectSubscription", projectUid.ToString());

      if (subscriptionUidAssigned != Guid.Empty)
      {
        await subsProxy.DissociateProjectSubscription(subscriptionUidAssigned,
          projectUid, Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Creates a geofence from the projects boundary
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    private async Task CreateGeofenceInGeofenceService(CreateProjectEvent project)
    {
      log.LogDebug($"Creating a geofence for project: {project.ProjectName}");
      var userUid = ((User as TIDCustomPrincipal).Identity as GenericIdentity).Name;

      try
      {
        geofenceUidCreated = await geofenceProxy.CreateGeofence(project.CustomerUID, project.ProjectName, "", "Project",
          project.ProjectBoundary,
          0, true, Guid.Parse(userUid), Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        await DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID).ConfigureAwait(false);
        await DissociateProjectSubscription(project.ProjectUID, subscriptionUidAssigned).ConfigureAwait(false);

        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,"geofenceProxy.CreateGeofenceInGeofenceService",e.Message);
      }
      if (geofenceUidCreated == Guid.Empty)
      {
        await DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID).ConfigureAwait(false);
        await DissociateProjectSubscription(project.ProjectUID, subscriptionUidAssigned).ConfigureAwait(false);

        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 59);
      }
    }

    #endregion private
  }
}