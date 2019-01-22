using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Filter.WebAPI.Controllers
{
  /// <summary>
  /// Base controller for all Filter service controller classes.
  /// </summary>
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// Gets the service's implementation of <see cref="IProjectListProxy"/>.
    /// </summary>
    protected readonly IProjectListProxy ProjectListProxy;

    /// <summary>
    /// Gets the service's Raptor interface controller.
    /// </summary>
    protected readonly IRaptorProxy RaptorProxy;

    /// <summary>
    /// Gets the service's configuration settings.
    /// </summary>
    protected readonly IConfigurationStore ConfigStore;

    /// <summary>
    /// Gets the service's Logger factory.
    /// </summary>
    protected readonly ILoggerFactory Logger;

    /// <summary>
    /// Gets the implmented Kafka Producer.
    /// </summary>
    protected readonly IKafka Producer;

    /// <summary>
    /// Gets or sets the service's log controller.
    /// </summary>
    protected ILogger Log;

    /// <summary>
    /// Gets the service's Kafka Topic.
    /// </summary>
    protected readonly string KafkaTopicName;

    /// <summary>
    /// Gets or sets the service's exception hander implementation of <see cref="IServiceExceptionHandler"/>.
    /// </summary>
    protected readonly IServiceExceptionHandler ServiceExceptionHandler;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IKafka producer, string eventType)
    {
      Logger = logger;
      Log = logger.CreateLogger<BaseController>();

      ConfigStore = configStore;
      ServiceExceptionHandler = serviceExceptionHandler;
      ProjectListProxy = projectListProxy;
      RaptorProxy = raptorProxy;
      Producer = producer;

      if (!Producer.IsInitializedProducer)
      {
        Producer.InitProducer(configStore);
      }

      KafkaTopicName = $"VSS.Interfaces.Events.MasterData.{eventType}{configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")}".Trim();
    }
  }
}
