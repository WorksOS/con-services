using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;

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
      IRaptorProxy raptorProxy = null, ISubscriptionProxy subscriptionProxy = null,
      ITransferProxy persistantTransferProxy = null, IFilterServiceProxy filterServiceProxy = null, ITRexImportFileProxy tRexImportFileProxy = null,
      IProjectRepository projectRepo = null, ISubscriptionRepository subscriptionRepo = null, IFileRepository fileRepo = null, 
      ICustomerRepository customerRepo = null, IHttpContextAccessor httpContextAccessor = null
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
        raptorProxy, subscriptionProxy, persistantTransferProxy, filterServiceProxy, tRexImportFileProxy,
        projectRepo, subscriptionRepo, fileRepo, customerRepo, httpContextAccessor
        );

      return executor;
    }
  }
}