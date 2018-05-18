using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ExecutorBaseTests
  {
    protected IConfigurationStore configStore;

    public IServiceProvider serviceProvider = null;
    protected AssetRepository assetRepository;
    protected DeviceRepository deviceRepository;
    protected CustomerRepository customerRepository;
    protected ProjectRepository projectRepository;
    protected SubscriptionRepository subscriptionRepository;
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();
    protected string kafkaTopicName;

    [TestInitialize]
    public virtual void InitTest()
    {
      const string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceProvider = serviceCollection.BuildServiceProvider();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
     
      assetRepository = serviceProvider.GetRequiredService<IRepository<IAssetEvent>>() as AssetRepository;
      deviceRepository = serviceProvider.GetRequiredService<IRepository<IDeviceEvent>>() as DeviceRepository;
      customerRepository = serviceProvider.GetRequiredService<IRepository<ICustomerEvent>>() as CustomerRepository;
      projectRepository = serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      subscriptionRepository = serviceProvider.GetRequiredService<IRepository<ISubscriptionEvent>>() as SubscriptionRepository;

      kafkaTopicName = configStore.GetValueString("KAFKA_TOPIC_NAME_NOTIFICATIONS") +
                       configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }
  
  }
}
