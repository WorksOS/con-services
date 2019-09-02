using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Base for all Project controllers
  /// </summary>
  public class ProjectBaseController : BaseController<ProjectBaseController>
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
    public ProjectBaseController(IKafka producer, IConfigurationStore configStore, ISubscriptionProxy subscriptionProxy) : base (producer, configStore)
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
      var projects = (await ProjectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      Logger.LogInformation($"Project list contains {projects.Count} projects");
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
      (await SubscriptionRepo.GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date)
        .ConfigureAwait(false)).ToImmutableList();
    }
  }
}
