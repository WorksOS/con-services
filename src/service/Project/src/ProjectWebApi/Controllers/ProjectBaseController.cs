using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;

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
    protected readonly ISubscriptionProxy subscriptionProxy;

    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected Guid subscriptionUidAssigned = Guid.Empty;

    /// <summary>
    ///  Used for rollback
    /// </summary>
    protected Guid geofenceUidCreated = Guid.Empty;


    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBaseController"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="subscriptionRepo">The subscriptions repo.</param>
    /// <param name="fileRepo"></param>
    /// <param name="configStore">The configStore.</param>
    /// <param name="subscriptionProxy">The subs proxy.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceExceptionHandler">The ServiceException handler</param>
    /// <param name="log"></param>
    /// <param name="dataOceanClient"></param>
    public ProjectBaseController(IKafka producer, 
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo, IFileRepository fileRepo,
      IConfigurationStore configStore, 
      ISubscriptionProxy subscriptionProxy, IRaptorProxy raptorProxy,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, ILogger log, IDataOceanClient dataOceanClient)
      : base(log, configStore, serviceExceptionHandler, producer, raptorProxy, projectRepo, subscriptionRepo, fileRepo, dataOceanClient)
    {
      this.subscriptionProxy = subscriptionProxy;
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

    /// <summary>
    /// Gets the free subscription regardless project type.
    /// </summary>
    /// <param name="customerUid">The customer uid.</param>
    /// <returns></returns>
    protected async Task<ImmutableList<Subscription>> GetFreeSubs(string customerUid)
    {
      return
      (await subscriptionRepo.GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date)
        .ConfigureAwait(false)).ToImmutableList();
    }
  }
}