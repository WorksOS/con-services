using System.Security.Claims;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Repositories;

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
      IProjectRepository projectRepo, IKafka producer = null, string kafkaTopicName = null) 
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      var executor = new TExecutor();

      executor.Initialise(
        log,
        configStore,
        serviceExceptionHandler,
        projectRepo,
        producer,
        kafkaTopicName
        );

      return executor;
    }
  }
}