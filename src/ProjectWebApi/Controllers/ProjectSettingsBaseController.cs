using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Repositories;
using VSS.MasterDataProxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// ProjectSettingsBaseController controller
  /// </summary>
  public class ProjectSettingsBaseController : Controller
  {
    /// <summary>
    /// The MasterData ProjectRepository persistent storage interface.
    /// </summary>
    protected ProjectRepository projectRepo;

    /// <summary>
    /// The Raptor Proxy interface.
    /// </summary>
    protected IRaptorProxy raptorProxy;

    /// <summary>
    /// The configuration configStore for module meta data.
    /// </summary>
    protected IConfigurationStore configStore;

    /// <summary>
    /// The logging system type for ILogger implementation instances.
    /// </summary>
    protected ILoggerFactory logger;

    /// <summary>
    /// The service exception handler.
    /// </summary>
    protected IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// The Kafka consumer.
    /// </summary>
    protected IKafka producer;

    /// <summary>
    /// The Kafka topic name used by the service.
    /// </summary>
    protected string kafkaTopicName;

    private readonly ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectSettingsBaseController"/> class.
    /// </summary>
    public ProjectSettingsBaseController(IRepository<IProjectEvent> projectRepo, IRaptorProxy raptorProxy,
      IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IKafka producer)
    {
      this.projectRepo = projectRepo as ProjectRepository;
      this.raptorProxy = raptorProxy;
      this.configStore = configStore;
      this.logger = logger;
      log = logger.CreateLogger<ProjectSettingsBaseController>();
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.producer = producer;
      if (!this.producer.IsInitializedProducer)
        this.producer.InitProducer(configStore);
    }
  }
}