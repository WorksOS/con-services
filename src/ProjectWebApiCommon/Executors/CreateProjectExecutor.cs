using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which creates a project - appropriate for v2 and v4 controllers
  /// </summary>
  public class CreateProjectExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected Guid subscriptionUidAssigned = Guid.Empty;

    /// <summary>
    ///
    /// </summary>
    protected Guid geofenceUidCreated = Guid.Empty;

    /// <summary>
    /// Processes the CreateProjectEvent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      CreateProjectEvent createProjectEvent = item as CreateProjectEvent;
      if (createProjectEvent == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
      }
      
      ProjectBoundaryValidator.ValidateWKT(createProjectEvent.ProjectBoundary);
      await ProjectRequestHelper.ValidateCoordSystemInRaptor(createProjectEvent,
        serviceExceptionHandler, customHeaders, raptorProxy).ConfigureAwait(false);

      log.LogDebug($"Testing if there are overlapping projects for project {createProjectEvent.ProjectName}");
      await ProjectRequestHelper.DoesProjectOverlap(createProjectEvent.CustomerUID.ToString(), createProjectEvent.ProjectUID.ToString(), 
        createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate, createProjectEvent.ProjectBoundary, 
        log, serviceExceptionHandler, projectRepo);

      AssociateProjectCustomer customerProject = new AssociateProjectCustomer
      {
        CustomerUID = createProjectEvent.CustomerUID,
        LegacyCustomerID = createProjectEvent.CustomerID,
        ProjectUID = createProjectEvent.ProjectUID,
        RelationType = RelationType.Owner,
        ActionUTC = createProjectEvent.ActionUTC,
        ReceivedUTC = createProjectEvent.ReceivedUTC
      };
      ProjectDataValidator.Validate(customerProject, projectRepo);

      // now making changes, potentially needing rollback 
      createProjectEvent = await CreateProjectInDb(createProjectEvent, customerProject).ConfigureAwait(false);
      await ProjectRequestHelper.CreateCoordSystemInRaptorAndTcc(
        createProjectEvent.ProjectUID, createProjectEvent.ProjectID, createProjectEvent.CoordinateSystemFileName, 
        createProjectEvent.CoordinateSystemFileContent, true, log, serviceExceptionHandler, customerUid, customHeaders,
        projectRepo, raptorProxy, configStore, fileRepo).ConfigureAwait(false);

      await AssociateProjectSubscriptionInSubscriptionService(createProjectEvent).ConfigureAwait(false);
      var geofenceUid = await CreateGeofenceInGeofenceService(createProjectEvent).ConfigureAwait(false);
      AssociateProjectGeofence associateProjectGeofence = null;
      if (geofenceUid != Guid.Empty) // TBC work-around 
      {
        associateProjectGeofence = new AssociateProjectGeofence()
        {
          ProjectUID = createProjectEvent.ProjectUID,
          GeofenceUID = geofenceUid,
          ActionUTC = DateTime.UtcNow
        };
        await AssociateProjectGeofence(associateProjectGeofence).ConfigureAwait(false);
      }

      // doing this as late as possible in case something fails. We can't cleanup kafka que.
      CreateKafkaEvents(createProjectEvent, customerProject, associateProjectGeofence);

      log.LogDebug("CreateProjectV4. completed succesfully");
      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Creates a project. Handles both old and new project boundary formats.
    /// </summary>
    /// <param name="project">The create project event</param>
    /// <param name="customerProject"></param>
    /// <returns></returns>
    private async Task<CreateProjectEvent> CreateProjectInDb(CreateProjectEvent project,
      AssociateProjectCustomer customerProject)
    {
      log.LogDebug(
        $"Creating the project in the DB {JsonConvert.SerializeObject(project)} and customerProject {JsonConvert.SerializeObject(customerProject)}");

      var isCreated = await projectRepo.StoreEvent(project).ConfigureAwait(false);
      if (isCreated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 61);

      log.LogDebug(
        $"Created the project in DB. IsCreated: {isCreated}. projectUid: {project.ProjectUID} legacyprojectID: {project.ProjectID}");

      if (project.ProjectID <= 0)
      {
        var existing = await projectRepo.GetProjectOnly(project.ProjectUID.ToString()).ConfigureAwait(false);
        if (existing != null && existing.LegacyProjectID > 0)
          project.ProjectID = existing.LegacyProjectID;
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 42);
        }
      }

      log.LogDebug($"Using Legacy projectId {project.ProjectID} for project {project.ProjectName}");

      // this is needed so that when ASNode (raptor client), which is called from CoordinateSystemPost, can retrieve the just written project+cp
      isCreated = await projectRepo.StoreEvent(customerProject).ConfigureAwait(false);

      if (isCreated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 63);

      log.LogDebug($"Created CustomerProject in DB {JsonConvert.SerializeObject(customerProject)}");
      return project; // legacyID may have been added
    }

    /// <summary>
    /// Validates if there any subscriptions available for the request create project event
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    private async Task AssociateProjectSubscriptionInSubscriptionService(CreateProjectEvent project)
    {
      if (project.ProjectType == ProjectType.LandFill || project.ProjectType == ProjectType.ProjectMonitoring)
      {
        subscriptionUidAssigned = Guid.Parse((await GetFreeSubs(customerUid, project.ProjectType, project.ProjectUID))
          .First().SubscriptionUID);
        log.LogDebug($"Received {subscriptionUidAssigned} subscription");
        //Assign a new project to a subscription
        try
        {
          // rethrows any exception
          await subscriptionProxy.AssociateProjectSubscription(subscriptionUidAssigned,
            project.ProjectUID, customHeaders).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          // this is only called from a Create, so no need to consider Update
          await ProjectRequestHelper.DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID, log, projectRepo).ConfigureAwait(false);

          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,
            "SubscriptionProxy.AssociateProjectSubscriptionInSubscriptionService", e.Message);
        }
      }
    }

    /// <summary>
    /// Gets the free subs for a project type
    /// </summary>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="type">The type.</param>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    private async Task<ImmutableList<Subscription>> GetFreeSubs(string customerUid, ProjectType type, Guid projectUid)
    {
      var availableFreSub =
        (await subscriptionRepo.GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date)
          .ConfigureAwait(false))
        .Where(s => s.ServiceTypeID == (int)type.MatchSubscriptionType()).ToImmutableList();

      log.LogDebug($"We have {availableFreSub.Count} free subscriptions for the selected project type {type}");
      if (!availableFreSub.Any())
      {
        await ProjectRequestHelper.DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid, log, projectRepo).ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 37);
      }

      return availableFreSub;
    }

    /// <summary>
    /// Creates a geofence from the projects boundary
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    protected async Task<Guid> CreateGeofenceInGeofenceService(CreateProjectEvent project)
    {
      // This is a temporary work-around of UserAuthorization issue with external applications.
      //     GeofenceService contains UserAuthorization which will fail for TBC, which uses the v2 API
      if (httpContextAccessor != null && httpContextAccessor.HttpContext.Request.Path.Value.Contains("api/v2/projects"))
      {
        log.LogWarning($"Skip creating a geofence for project: {project.ProjectName}, as request has come from the TBC endpoint: {httpContextAccessor.HttpContext.Request.Path.Value}.");
        return Guid.Empty;
      }

      log.LogDebug($"Creating a geofence for project: {project.ProjectName}");

      try
      {
        var area = ProjectBoundaryValidator.CalculateAreaSqMeters(project.ProjectBoundary);

        geofenceUidCreated = await geofenceProxy.CreateGeofence(project.CustomerUID, project.ProjectName, "", "Project",
          project.ProjectBoundary,
          0, true, Guid.Parse(userId), area, customHeaders).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        await ProjectRequestHelper.DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID, log, projectRepo).ConfigureAwait(false);
        await DissociateProjectSubscription(project.ProjectUID, subscriptionUidAssigned).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,
          "geofenceProxy.CreateGeofenceInGeofenceService", e.Message);
      }

      if (geofenceUidCreated == Guid.Empty)
      {
        await ProjectRequestHelper.DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID, log, projectRepo).ConfigureAwait(false);
        await DissociateProjectSubscription(project.ProjectUID, subscriptionUidAssigned).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 59);
      }

      return geofenceUidCreated;
    }

    /// <summary>
    /// Associates project with its geofence.
    ///      projectService uses Project.ProjectBoundary, 
    ///      however other services use the ProjectGeofence/Geofence association
    /// </summary>
    /// <param name="projectGeofence">The geofence project.</param>
    /// <returns></returns>
    protected async Task AssociateProjectGeofence(AssociateProjectGeofence projectGeofence)
    {
      ProjectDataValidator.Validate(projectGeofence, projectRepo);
      projectGeofence.ReceivedUTC = DateTime.UtcNow;

      var isUpdated = await projectRepo.StoreEvent(projectGeofence).ConfigureAwait(false);
      if (isUpdated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 65);
    }

    /// <summary>
    /// Creates Kafka events.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="customerProject">The create projectCustomer event</param>
    /// <param name="projectGeofence"></param>
    /// <returns></returns>
    protected void CreateKafkaEvents(CreateProjectEvent project, AssociateProjectCustomer customerProject, AssociateProjectGeofence projectGeofence)
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

      if (projectGeofence != null)
      {
        log.LogDebug($"AssociateProjectGeofenceEvent on kafka queue {JsonConvert.SerializeObject(projectGeofence)}");

        var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectGeofence = projectGeofence});
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>()
          {
            new KeyValuePair<string, string>(projectGeofence.ProjectUID.ToString(), messagePayload)
          });
      }
    }

    #region rollback
    /// <summary>
    /// rolls back the ProjectSubscription association made, due to a subsequent error
    /// </summary>
    /// <returns></returns>
    protected async Task DissociateProjectSubscription(Guid projectUid, Guid subscriptionUidAssigned)
    {
      if (subscriptionUidAssigned != Guid.Empty)
      {
        await subscriptionProxy.DissociateProjectSubscription(subscriptionUidAssigned,
          projectUid, customHeaders).ConfigureAwait(false);
      }
    }

    #endregion rollback

  }
}