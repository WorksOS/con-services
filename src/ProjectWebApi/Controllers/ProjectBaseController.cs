using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Base for all Project controllers
  /// </summary>
  public class ProjectBaseController : BaseController
  {
     /// <summary>
    /// Gets or sets the subscription proxy.
    /// </summary>
    protected readonly ISubscriptionProxy subsProxy;

    /// <summary>
    /// Gets or sets the Geofence proxy. 
    /// </summary>
    protected readonly IGeofenceProxy geofenceProxy;

    /// <summary>
    /// Gets or sets the Subscription Repository.
    /// </summary>
    protected readonly SubscriptionRepository subsService;

    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected Guid subscriptionUidAssigned = Guid.Empty;

    /// <summary>
    ///
    /// </summary>
    protected Guid geofenceUidCreated = Guid.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBaseController"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="subscriptionsRepo">The subscriptions repo.</param>
    /// <param name="configStore">The configStore.</param>
    /// <param name="subsProxy">The subs proxy.</param>
    /// <param name="geofenceProxy">The geofence proxy.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceExceptionHandler">The ServiceException handler</param>
    /// <param name="log"></param>
    public ProjectBaseController(IKafka producer, IRepository<IProjectEvent> projectRepo, 
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore configStore, 
      ISubscriptionProxy subsProxy, IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, 
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, ILogger log)
      : base(log, configStore, serviceExceptionHandler, producer, raptorProxy, projectRepo)
    {
      subsService = subscriptionsRepo as SubscriptionRepository;
      this.subsProxy = subsProxy;
      this.geofenceProxy = geofenceProxy;
    }

    /// <summary>
    /// Gets the project list for a customer
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<Repositories.DBModels.Project>> GetProjectList()
    {
      var customerUid = LogCustomerDetails("GetProjectList", "");
      var projects = (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      log.LogInformation($"Project list contains {projects.Count} projects");
      return projects;
    }


    #region privateValidation

    /// <summary>
    /// Determines if the project boundary overlaps any exising project for the customer in time and space.
    ///    not needed for v1 or 3 and they come via CGen which already does this overlap checking.
    /// </summary>    
    /// <param name="project">The create project event</param>
    /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
    /// <returns></returns>
    protected async Task<bool> DoesProjectOverlap(CreateProjectEvent project, string databaseProjectBoundary)
    {
      var overlaps =
        await projectRepo.DoesPolygonOverlap(project.CustomerUID.ToString(), databaseProjectBoundary,
          project.ProjectStartDate, project.ProjectEndDate).ConfigureAwait(false);
      if (overlaps)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 43);

      log.LogDebug($"No overlapping projects for {project.ProjectName}");
      return overlaps;
    }

    /// <summary>
    /// validate CordinateSystem if provided
    /// </summary>
    protected async Task<bool> ValidateCoordSystemInRaptor(IProjectEvent project)
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
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 45);
      }

      if (project is CreateProjectEvent)
        ProjectBoundaryValidator.ValidateWKT(((CreateProjectEvent)project).ProjectBoundary);

      var csFileName = project is CreateProjectEvent
        ? ((CreateProjectEvent)project).CoordinateSystemFileName
        : ((UpdateProjectEvent)project).CoordinateSystemFileName;
      var csFileContent = project is CreateProjectEvent
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
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.CoordinateSystemValidate", e.Message);
        }
        if (coordinateSystemSettingsResult == null)
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 46);

        if (coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 47, coordinateSystemSettingsResult.Code.ToString(),
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
    protected async Task<CreateProjectEvent> CreateProjectInDb(CreateProjectEvent project, AssociateProjectCustomer customerProject)
    {
      log.LogDebug($"Creating the project in the DB {JsonConvert.SerializeObject(project)} and customerProject {JsonConvert.SerializeObject(customerProject)}");

      var isCreated = await projectRepo.StoreEvent(project).ConfigureAwait(false);
      if (isCreated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 61);

      log.LogDebug($"Created the project in DB. IsCreated: {isCreated}. projectUid: {project.ProjectUID} legacyprojectID: {project.ProjectID}");

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

    protected async Task CreateCoordSystemInRaptor(Guid projectUid, int legacyProjectId, string coordinateSystemFileName, byte[] coordinateSystemFileContent, bool isCreate)
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
              await DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid).ConfigureAwait(false);

            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 41, (coordinateSystemSettingsResult?.Code ?? -1).ToString(), coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
          }
        }
        catch (Exception e)
        {
          if (isCreate)
            await DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid).ConfigureAwait(false);

          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.CoordinateSystemPost", e.Message);
        }
      }
    }

    /// <summary>
    /// Used internally, if a step fails, after a project has been CREATED, 
    ///    then delete it permanently i.e. don't just set IsDeleted.
    /// Since v4 CreateProjectInDB also associates projectCustomer then roll this back also.
    /// DissociateProjectCustomer actually deletes the DB ent4ry
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    private async Task DeleteProjectPermanentlyInDb(Guid customerUid, Guid projectUid)
    {
      log.LogDebug($"DeleteProjectPermanentlyInDB: {projectUid}");
      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = projectUid,
        DeletePermanently = true,
        ActionUTC = DateTime.UtcNow
      };
      await projectRepo.StoreEvent(deleteProjectEvent).ConfigureAwait(false);

      await projectRepo.StoreEvent(new DissociateProjectCustomer
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
    protected void CreateKafkaEvents(CreateProjectEvent project, AssociateProjectCustomer customerProject)
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
    /// <param name="projectUid"></param>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    /// <exception cref="ContractExecutionResult">No available subscriptions for the selected customer</exception>
    protected async Task<ImmutableList<Subscription>> GetFreeSubs(string customerUid, ProjectType type, Guid projectUid)
    {
      var availableFreSub =
        (await subsService.GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date).ConfigureAwait(false))
        .Where(s => s.ServiceTypeID == (int)type.MatchSubscriptionType()).ToImmutableList();

      log.LogDebug($"We have {availableFreSub.Count} free subscriptions for the selected project type {type}");
      if (!availableFreSub.Any())
      {
        await DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid).ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 37);
      }
      return availableFreSub;
    }

    /// <summary>
    /// Gets the free subscription regardless project type.
    /// </summary>
    /// <param name="customerUid">The customer uid.</param>
    /// <returns></returns>
    protected async Task<ImmutableList<Subscription>> GetFreeSubs(string customerUid)
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
    protected async Task AssociateProjectSubscriptionInSubscriptionService(CreateProjectEvent project)
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

          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "ubscriptionProxy.AssociateProjectSubscriptionInSubscriptionService", e.Message);
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
    protected async Task CreateGeofenceInGeofenceService(CreateProjectEvent project)
    {
      log.LogDebug($"Creating a geofence for project: {project.ProjectName}");

      try
      {
        var area = ProjectBoundaryValidator.CalculateAreaSqMeters(project.ProjectBoundary);

        geofenceUidCreated = await geofenceProxy.CreateGeofence(project.CustomerUID, project.ProjectName, "", "Project",
          project.ProjectBoundary,
          0, true, Guid.Parse(userId), area, Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        await DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID).ConfigureAwait(false);
        await DissociateProjectSubscription(project.ProjectUID, subscriptionUidAssigned).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "geofenceProxy.CreateGeofenceInGeofenceService", e.Message);
      }
      if (geofenceUidCreated == Guid.Empty)
      {
        await DeleteProjectPermanentlyInDb(project.CustomerUID, project.ProjectUID).ConfigureAwait(false);
        await DissociateProjectSubscription(project.ProjectUID, subscriptionUidAssigned).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 59);
      }
    }


    #endregion private

  }
}