using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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
      var customerUid = LogCustomerDetails("GetProjectList");
      var projects = (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      log.LogInformation($"Project list contains {projects.Count} projects");
      return projects;
    }  
  }
}