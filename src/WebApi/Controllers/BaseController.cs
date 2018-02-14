using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// Gets or sets the local log provider.
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Gets or sets the Configuration Store. 
    /// </summary>
    protected readonly IConfigurationStore configStore;

    /// <summary>
    /// Gets or sets the Kafak consumer.
    /// </summary>
    protected readonly IKafka producer;

    /// <summary>
    /// Gets or sets the Kafka topic.
    /// </summary>
    protected readonly string kafkaTopicName;

    protected AssetRepository assetRepository;
    protected DeviceRepository deviceRepository;
    protected CustomerRepository customerRepository;
    protected ProjectRepository projectRepository;
    protected SubscriptionRepository subscriptionsRepository;

    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore, 
      IRepository<IAssetEvent> assetRepository, IRepository<IDeviceEvent> deviceRepository,
      IRepository<ICustomerEvent> customerRepository, IRepository<IProjectEvent> projectRepository,
      IRepository<ISubscriptionEvent> subscriptionsRepository, IKafka producer)
    {
      this.log = logger.CreateLogger<BaseController>();
      this.configStore = configStore;
      this.assetRepository = assetRepository as AssetRepository;
      this.deviceRepository = deviceRepository as DeviceRepository;
      this.customerRepository = customerRepository as CustomerRepository;
      this.projectRepository = projectRepository as ProjectRepository;
      this.subscriptionsRepository = subscriptionsRepository as SubscriptionRepository;

      //temp fix as TFProcessor was inadvertently changed to call Notification v2 endpoint
      //this.producer = producer;
      //if (!this.producer.IsInitializedProducer)
      //  this.producer.InitProducer(configStore);

      //kafkaTopicName = configStore.GetValueString("KAFKA_TOPIC_NAME_NOTIFICATIONS") +
      //                 configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }
  }
}
