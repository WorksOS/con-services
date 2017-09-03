using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Repositories;
using VSS.MasterDataProxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(
      ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler,
      string customerUid, string userId = null, string userEmailAddress = null, IDictionary<string, string> headers = null,
      IKafka producer = null, string kafkaTopicName = null,
      IGeofenceProxy geofenceProxy = null, IRaptorProxy raptorProxy = null, ISubscriptionProxy subscriptionProxy = null,
      IProjectRepository projectRepo = null, IRepository<ISubscriptionEvent> subscriptionsRepo = null
      ) 
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      var executor = new TExecutor();

      executor.Initialise(
        log, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, headers,
        producer, kafkaTopicName,
        geofenceProxy, raptorProxy, subscriptionProxy,
        projectRepo, subscriptionsRepo
        );

      return executor;
    }
  }
}