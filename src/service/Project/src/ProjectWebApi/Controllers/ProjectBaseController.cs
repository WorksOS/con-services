using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

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
    public ProjectBaseController(IKafka producer, 
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo, IFileRepository fileRepo,
      IConfigurationStore configStore, 
      ISubscriptionProxy subscriptionProxy, IProductivity3dProxy productivity3DProxy,
      ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler,  
      IDataOceanClient dataOceanClient,
      ITPaaSApplicationAuthentication authn)
      : base(loggerFactory, configStore, serviceExceptionHandler, producer, productivity3DProxy, projectRepo, 
        subscriptionRepo, fileRepo, dataOceanClient, authn)
    {
      this.subscriptionProxy = subscriptionProxy;
    }

    /// <summary>
    /// Gets the project list for a customer
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<ProjectDatabaseModel>> GetProjectList()
    {
      var customerUid = LogCustomerDetails("GetProjectList", "");
      var projects = (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      logger.LogInformation($"Project list contains {projects.Count} projects");
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
