using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Productivity3D.MasterDataProxies.Interfaces;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.Repo;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.ProjectWebApi.Controllers
{
  /// <summary>
  /// ProjectSettingsBaseController controller
  /// </summary>
  public class ProjectSettingsBaseController : Controller
  {
    protected ProjectRepository projectRepo;
    protected IRaptorProxy raptorProxy;
    protected IConfigurationStore configStore;
    protected ILoggerFactory logger;
    private readonly ILogger log;
    protected IServiceExceptionHandler serviceExceptionHandler;
    protected IKafka producer;

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