using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Repository;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ExecutorBaseTests
  {
    protected IConfigurationStore ConfigStore;

    protected IServiceProvider ServiceProvider;
    protected AssetRepository AssetRepository;
    protected DeviceRepository DeviceRepository;
    protected CustomerRepository CustomerRepository;
    protected ProjectRepository ProjectRepository;
    protected SubscriptionRepository SubscriptionRepository;
    protected static ContractExecutionStatesEnum ContractExecutionStatesEnum = new ContractExecutionStatesEnum();
    protected string KafkaTopicName;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();
      
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.TagFileAuth.WepApiTests.log")));
      serviceCollection.AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>();
      ServiceProvider = serviceCollection.BuildServiceProvider();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
     
      AssetRepository = ServiceProvider.GetRequiredService<IRepository<IAssetEvent>>() as AssetRepository;
      DeviceRepository = ServiceProvider.GetRequiredService<IRepository<IDeviceEvent>>() as DeviceRepository;
      CustomerRepository = ServiceProvider.GetRequiredService<IRepository<ICustomerEvent>>() as CustomerRepository;
      ProjectRepository = ServiceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      SubscriptionRepository = ServiceProvider.GetRequiredService<IRepository<ISubscriptionEvent>>() as SubscriptionRepository;

      KafkaTopicName = ConfigStore.GetValueString("KAFKA_TOPIC_NAME_NOTIFICATIONS") +
                       ConfigStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }
  
  }
}
