using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.MasterDataConsumer.Tests
{
  [TestClass]
  public class LoggingTests
  {
    private IServiceProvider serviceProvider;

    [TestMethod]
    public void CanConstructFromDI()
    {
      CreateCollection(true);

      var assetConsumer = serviceProvider.GetService<IKafkaConsumer<IAssetEvent>>();
      Assert.IsNotNull(assetConsumer);

      var customerConsumer = serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>();
      Assert.IsNotNull(customerConsumer);

      var deviceConsumer = serviceProvider.GetService<IKafkaConsumer<IDeviceEvent>>();
      Assert.IsNotNull(deviceConsumer);

      var geofenceConsumer = serviceProvider.GetService<IKafkaConsumer<IGeofenceEvent>>();
      Assert.IsNotNull(geofenceConsumer);

      var projectConsumer = serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
      Assert.IsNotNull(projectConsumer);

      var subscriptionConsumer = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
      Assert.IsNotNull(subscriptionConsumer);
    }

    [TestMethod]
    public void CannotConstructFromDI()
    {
      CreateCollection(false);

      var ex = Assert.ThrowsException<ArgumentNullException>(() => serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>());
      Assert.AreEqual("Value cannot be null.\r\nParameter name: provider",
                      ex.Message);
    }

    [TestMethod]
    public void ConstructLoggerNameFromKafkaTopic()
    {
      string[] kafkaTopic = { "VSS.Interfaces.Events.MasterData.ICustomerEvent", "VSS.Interfaces.Events.MasterData.IAssetEvent" };

      var eventType = kafkaTopic[0].Split('.').Last();
      var loggerRepoName = "MDC " + eventType;
      Assert.AreEqual("MDC ICustomerEvent", loggerRepoName, "loggerName incorrect");
    }

    private void CreateCollection(bool withLogging)
    {
      var serviceCollection = new ServiceCollection()
          .AddTransient<IKafka, RdKafkaDriver>()

          .AddTransient<IKafkaConsumer<IAssetEvent>, KafkaConsumer<IAssetEvent>>()
          .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
          .AddTransient<IKafkaConsumer<IDeviceEvent>, KafkaConsumer<IDeviceEvent>>()
          .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
          .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
          .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
          .AddTransient<IKafkaConsumer<IFilterEvent>, KafkaConsumer<IFilterEvent>>()
          .AddTransient<IMessageTypeResolver, MessageResolver>()
          .AddTransient<IRepositoryFactory, RepositoryFactory>()

          .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
          .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddSingleton<IConfigurationStore, GenericConfiguration>();

      if (withLogging)
      {
        serviceProvider = serviceCollection
          .AddLogging()
          .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("MasterDataConsumerTests.log")))
          .BuildServiceProvider();
      }
    }
  }
}
