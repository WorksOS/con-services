using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Filters;
using ProjectWebApiCommon.Models;
using Repositories;
using Repositories.DBModels;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApiCommon.ResultsHandling;

namespace Controllers
{
  /// <summary>
  /// Project Base for all Project controllers
  /// </summary>
  public class ProjectBaseController : Controller
  {
    protected readonly IKafka producer;
    protected readonly ILogger log;
    protected readonly ISubscriptionProxy subsProxy;
    protected readonly IGeofenceProxy geofenceProxy;
    protected readonly IRaptorProxy raptorProxy;

    protected readonly ProjectRepository projectService;
    protected readonly IConfigurationStore store;
    protected readonly string kafkaTopicName;
    private readonly SubscriptionRepository subsService;
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBaseController"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="subscriptionsRepo">The subscriptions repo.</param>
    /// <param name="store">The store.</param>
    /// <param name="subsProxy">The subs proxy.</param>
    /// <param name="geofenceProxy">The geofence proxy.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="fileRepo">For TCC file transfer</param>
    public ProjectBaseController(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger)
    {
      log = logger.CreateLogger<ProjectBaseController>();
      this.producer = producer;
      //We probably want to make this thing singleton?
      if (!this.producer.IsInitializedProducer)
        this.producer.InitProducer(store);
      //TODO change this pattern, make it safer
      projectService = projectRepo as ProjectRepository;
      subsService = subscriptionsRepo as SubscriptionRepository;
      this.subsProxy = subsProxy;
      this.geofenceProxy = geofenceProxy;
      this.raptorProxy = raptorProxy;
      this.store = store;

      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       store.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }

    /// <summary>
    /// Validates if there any subscriptions available for the request create project event
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    protected async Task ValidateAssociateSubscriptions(CreateProjectEvent project)
    {
      log.LogDebug("ValidateAssociateSubscriptions");
      var customerUid = (User as TidCustomPrincipal).CustomerUid;
      log.LogDebug($"CustomerUID={customerUid} and user={User}");

      //Apply here rules validating types of projects I'm able to create (i.e. LF only if there is one available LF subscription available) Performance is not a concern as this request is executed once in a blue moon
      //Retrieve available subscriptions
      //Should be Today used or UTC?

      //let's find out here what project we can create
      if (project.ProjectType == ProjectType.LandFill || project.ProjectType == ProjectType.ProjectMonitoring)
      {
        var availableFreeSub = (await GetFreeSubs(customerUid, project.ProjectType)).First();
        log.LogDebug($"Receieved {availableFreeSub.SubscriptionUID} subscription");
        //Assign a new project to a subs
        await subsProxy.AssociateProjectSubscription(Guid.Parse(availableFreeSub.SubscriptionUID),
          project.ProjectUID,
          Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Gets the free subs for a project type
    /// </summary>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    /// <exception cref="ContractExecutionResult">No available subscriptions for the selected customer</exception>
    protected async Task<ImmutableList<Subscription>> GetFreeSubs(string customerUid, ProjectType type)
    {
      var availableSubscriptions =
        (await subsService.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date).ConfigureAwait(false))
        .Where(s => s.ServiceTypeID == (int) type.MatchSubscriptionType()).ToImmutableList();
      log.LogDebug(
        $"Receieved {availableSubscriptions.Count()} subscriptions with contents {JsonConvert.SerializeObject(availableSubscriptions)}");
      var projects =
        (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      log.LogDebug($"Receieved {projects.Count()} projects with contents {JsonConvert.SerializeObject(projects)}");

      var availableFreSub = availableSubscriptions
        .Where(s => !projects
                      .Where(p => p.ProjectType == type && !p.IsDeleted)
                      .Select(p => p.SubscriptionUID)
                      .Contains(s.SubscriptionUID) &&
                    s.ServiceTypeID == (int) type.MatchSubscriptionType())
        .ToImmutableList();
      log.LogDebug(
        $"We have {availableFreSub.Count} free subscriptions for the selected project type {type.ToString()}");
      if (!availableFreSub.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(37),
            contractExecutionStatesEnum.FirstNameWithOffset(37)));
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
      var availableSubscriptions =
        (await subsService.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date).ConfigureAwait(false))
        .Where(s => s.ServiceTypeID == (int) ServiceTypeEnum.Landfill ||
                    s.ServiceTypeID == (int) ServiceTypeEnum.ProjectMonitoring);
      var projects = await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false);

      var availableFreSub = availableSubscriptions
        .Where(s => !projects
          .Where(p => !p.IsDeleted)
          .Select(p => p.SubscriptionUID)
          .Contains(s.SubscriptionUID))
        .ToImmutableList();

      return availableFreSub;
    }


    /// <summary>
    /// Gets the project list for a customer
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<Repositories.DBModels.Project>> GetProjectList()
    {
      var customerUid = (User as TidCustomPrincipal).CustomerUid;
      log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);
      var projects = (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      log.LogInformation($"Project list contains {projects.Count()} projects");
      return projects;
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    protected async Task<Repositories.DBModels.Project> GetProject(string projectUid)
    {
      var customerUid = (User as TidCustomPrincipal).CustomerUid;
      log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);
      var project =
        (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));

      if (project == null)
      {
        log.LogWarning($"User doesn't have access to {projectUid}");
        throw new ServiceException(HttpStatusCode.Forbidden,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(1),
            contractExecutionStatesEnum.FirstNameWithOffset(1)));
      }

      log.LogInformation($"Project {projectUid} retrieved");
      return project;
    }


    /// <summary>
    /// Updates the project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    protected async Task UpdateProject(UpdateProjectEvent project)
    {
      ProjectDataValidator.Validate(project, projectService);
      project.ReceivedUTC = DateTime.UtcNow;

      var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = project});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });
      await projectService.StoreEvent(project).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a project. Handles both old and new project boundary formats. this method can be overriden
    /// </summary>
    /// <param name="project">The create project event</param>
    /// <param name="kafkaProjectBoundary">The project boundary in the old format (coords comma separated, points semicolon separated)</param>
    /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
    /// <returns></returns>
    protected virtual async Task CreateProject(CreateProjectEvent project, string kafkaProjectBoundary,
      string databaseProjectBoundary)
    {
      ProjectDataValidator.Validate(project, projectService);
      if (project.ProjectID <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(44),
            contractExecutionStatesEnum.FirstNameWithOffset(44)));
      }
      project.ReceivedUTC = DateTime.UtcNow;

      //Send boundary as old format on kafka queue
      project.ProjectBoundary = kafkaProjectBoundary;
      var messagePayload = JsonConvert.SerializeObject(new {CreateProjectEvent = project});
      await producer.Send(kafkaTopicName,
        new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload));
      //Save boundary as WKT
      project.ProjectBoundary = databaseProjectBoundary;
      await projectService.StoreEvent(project).ConfigureAwait(false);
    }

    /// <summary>
    /// Associates the project customer.
    /// </summary>
    /// <param name="customerProject">The customer project.</param>
    /// <returns></returns>
    protected async Task AssociateProjectCustomer(AssociateProjectCustomer customerProject)
    {
      ProjectDataValidator.Validate(customerProject, projectService);
      customerProject.ReceivedUTC = DateTime.UtcNow;

      var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectCustomer = customerProject});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
        });
      await projectService.StoreEvent(customerProject).ConfigureAwait(false);
    }

    /// <summary>
    /// Dissociates the project customer.
    /// </summary>
    /// <param name="customerProject">The customer project.</param>
    /// <returns></returns>
    protected async Task DissociateProjectCustomer(DissociateProjectCustomer customerProject)
    {
      ProjectDataValidator.Validate(customerProject, projectService);
      customerProject.ReceivedUTC = DateTime.UtcNow;

      var messagePayload = JsonConvert.SerializeObject(new {DissociateProjectCustomer = customerProject});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
        });
      await projectService.StoreEvent(customerProject).ConfigureAwait(false);
    }

    /// <summary>
    /// Associates the geofence project.
    /// </summary>
    /// <param name="geofenceProject">The geofence project.</param>
    /// <returns></returns>
    protected async Task AssociateGeofenceProject(AssociateProjectGeofence geofenceProject)
    {
      ProjectDataValidator.Validate(geofenceProject, projectService);
      geofenceProject.ReceivedUTC = DateTime.UtcNow;

      var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectGeofence = geofenceProject});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(geofenceProject.ProjectUID.ToString(), messagePayload)
        });
      await projectService.StoreEvent(geofenceProject).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    protected async Task DeleteProject(DeleteProjectEvent project)
    {
      ProjectDataValidator.Validate(project, projectService);
      project.ReceivedUTC = DateTime.UtcNow;

      var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });
      await projectService.StoreEvent(project).ConfigureAwait(false);
    }
  }
}
