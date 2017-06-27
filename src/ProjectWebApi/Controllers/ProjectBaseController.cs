using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Principal;
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
    protected readonly SubscriptionRepository subsService;
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    // save for potential rollback
    protected Guid subscriptionUidAssigned = Guid.Empty;

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
    /// Gets the project list for a customer
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<Repositories.DBModels.Project>> GetProjectList()
    {
      var customerUid = LogCustomerDetails("GetProjectList");
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
      var customerUid = LogCustomerDetails("GetProject", projectUid);
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
