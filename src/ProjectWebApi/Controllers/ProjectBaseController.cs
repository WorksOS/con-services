using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using MasterDataProxies.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApi.Filters;
using VSS.Productivity3D.ProjectWebApi.Internal;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.ProjectWebApi.Controllers
{
  /// <summary>
  /// Project Base for all Project controllers
  /// </summary>
  public class ProjectBaseController : Controller
  {
    /// <summary>
    /// Gets or sets the Kafak consumer.
    /// </summary>
    protected readonly IKafka producer;

    /// <summary>
    /// Gets or sets the local log provider.
    /// </summary>
    protected readonly ILogger log;

    /// <summary>
    /// Gets or sets the subscription proxy.
    /// </summary>
    protected readonly ISubscriptionProxy subsProxy;

    /// <summary>
    /// Gets or sets the Geofence proxy. 
    /// </summary>
    protected readonly IGeofenceProxy geofenceProxy;

    /// <summary>
    /// Gets or sets the Raptor proxy.
    /// </summary>
    protected readonly IRaptorProxy raptorProxy;

    /// <summary>
    /// Gets or sets the Project Repository. 
    /// </summary>
    protected readonly ProjectRepository projectService;

    /// <summary>
    /// Gets or sets the Configuration Store. 
    /// </summary>
    protected readonly IConfigurationStore store;

    /// <summary>
    /// Gets or sets the Kafka topic.
    /// </summary>
    protected readonly string kafkaTopicName;

    /// <summary>
    /// Gets or sets the Subscription Repository.
    /// </summary>
    protected readonly SubscriptionRepository subsService;

    /// <summary>
    /// Gets or sets the Service exception handler.
    /// </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler;

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
    /// <param name="store">The store.</param>
    /// <param name="subsProxy">The subs proxy.</param>
    /// <param name="geofenceProxy">The geofence proxy.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceExceptionHandler">The ServiceException handler</param>
    public ProjectBaseController(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
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

      ServiceExceptionHandler = serviceExceptionHandler;

      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       store.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }

    /// <summary>
    /// Gets the project list for a customer
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<Repositories.DBModels.Project>> GetProjectList()
    {
      var customerUid = LogCustomerDetails("GetProjectList");
      var projects = (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      log.LogInformation($"Project list contains {projects.Count} projects");
      return projects;
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    protected async Task<Repositories.DBModels.Project> GetProject(string projectUid)
    {
      var customerUid = LogCustomerDetails("GetProject", projectUid);
      var project =
        (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));

      if (project == null)
      {
        log.LogWarning($"User doesn't have access to {projectUid}");
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      log.LogInformation($"Project {projectUid} retrieved");
      return project;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="projectUid"></param>

    /// <returns></returns>
    protected string LogCustomerDetails(string functionName, string projectUid = "")
    {
      var customerUid = (User as TIDCustomPrincipal).CustomerUid;
      var userUid = ((User as TIDCustomPrincipal).Identity as GenericIdentity).Name;
      log.LogInformation($"{functionName}: UserUID={userUid}, CustomerUID={customerUid}  and projectUid={projectUid}");

      return customerUid;
    }
  }
}