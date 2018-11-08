using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Common controller base.
  /// </summary>
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// Gets or sets the Configuration Store. 
    /// </summary>
    protected readonly IConfigurationStore configStore;

    /// <summary>
    /// Gets the <see cref="AssetRepository"/> field. 
    /// </summary>
    protected AssetRepository assetRepository;

    /// <summary>
    /// Gets the <see cref="DeviceRepository"/> field. 
    /// </summary>
    protected DeviceRepository deviceRepository;

    /// <summary>
    /// Gets the <see cref="CustomerRepository"/> field. 
    /// </summary>
    protected CustomerRepository customerRepository;

    /// <summary>
    /// Gets the <see cref="ProjectRepository"/> field. 
    /// </summary>
    protected ProjectRepository projectRepository;

    /// <summary>
    /// Gets the <see cref="SubscriptionRepository"/> field. 
    /// </summary>
    protected SubscriptionRepository subscriptionsRepository;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore, 
      IRepository<IAssetEvent> assetRepository, IRepository<IDeviceEvent> deviceRepository,
      IRepository<ICustomerEvent> customerRepository, IRepository<IProjectEvent> projectRepository,
      IRepository<ISubscriptionEvent> subscriptionsRepository, IKafka producer)
    {
      this.configStore = configStore;
      this.assetRepository = assetRepository as AssetRepository;
      this.deviceRepository = deviceRepository as DeviceRepository;
      this.customerRepository = customerRepository as CustomerRepository;
      this.projectRepository = projectRepository as ProjectRepository;
      this.subscriptionsRepository = subscriptionsRepository as SubscriptionRepository;
    }
  }
}
