using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Repository;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.MasterDataConsumer.Tests
{

  [TestClass]
  public class MasterDataConsumerTests
  {
    private IServiceProvider serviceProvider;
    private string loggerRepoName = "UnitTestLogTest";

    [TestInitialize]
    public void InitTest()
    {
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceProvider = new ServiceCollection()
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

          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddLogging()
          .AddSingleton(loggerFactory)
          .BuildServiceProvider();
    }

    [TestMethod]
    public void CanCreateAssetEventConsumer()
    {
      var assetConsumer = serviceProvider.GetService<IKafkaConsumer<IAssetEvent>>();
      Assert.IsNotNull(assetConsumer);

      assetConsumer.SetTopic("VSS.Interfaces.Events.MasterData.IAssetEvent");
      var assetReturn = assetConsumer.StartProcessingAsync(new CancellationTokenSource());
      Assert.IsNotNull(assetReturn);
    }

    [TestMethod]
    public void CanCreateCustomerEventConsumer()
    {
      var customerConsumer = serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>();
      Assert.IsNotNull(customerConsumer);

      customerConsumer.SetTopic("VSS.Interfaces.Events.MasterData.ICustomerEvent");
      var customerReturn = customerConsumer.StartProcessingAsync(new CancellationTokenSource());
      Assert.IsNotNull(customerReturn);
    }

    [TestMethod]
    public void CanCreateDeviceEventConsumer()
    {
      var deviceConsumer = serviceProvider.GetService<IKafkaConsumer<IDeviceEvent>>();
      Assert.IsNotNull(deviceConsumer);

      deviceConsumer.SetTopic("VSS.Interfaces.Events.MasterData.IDeviceEvent");
      var deviceReturn = deviceConsumer.StartProcessingAsync(new CancellationTokenSource());
      Assert.IsNotNull(deviceReturn);
    }

    [TestMethod]
    public void CanCreateGeofenceEventConsumer()
    {
      var geofenceConsumer = serviceProvider.GetService<IKafkaConsumer<IGeofenceEvent>>();
      Assert.IsNotNull(geofenceConsumer);

      geofenceConsumer.SetTopic("VSS.Interfaces.Events.MasterData.IGeofenceEvent");
      var geofenceReturn = geofenceConsumer.StartProcessingAsync(new CancellationTokenSource());
      Assert.IsNotNull(geofenceReturn);
    }

    [TestMethod]
    public void CanCreateProjectEventConsumer()
    {
      var projectConsumer = serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
      Assert.IsNotNull(projectConsumer);

      projectConsumer.SetTopic("VSS.Interfaces.Events.MasterData.IProjectEvent");
      var projectReturn = projectConsumer.StartProcessingAsync(new CancellationTokenSource());
      Assert.IsNotNull(projectReturn);
    }

    [TestMethod]
    public void CanCreateSubscriptionEventConsumer()
    {
      var subscriptionConsumer = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
      Assert.IsNotNull(subscriptionConsumer);

      subscriptionConsumer.SetTopic("VSS.Interfaces.Events.MasterData.ISubscriptionEvent");
      var subscriptionReturn = subscriptionConsumer.StartProcessingAsync(new CancellationTokenSource());
      Assert.IsNotNull(subscriptionReturn);
    }

    [TestMethod]
    public void CanHandleUnsupportedMessageType()
    {
      var messageResolver = serviceProvider.GetService<IMessageTypeResolver>();
      var converter = messageResolver.GetConverter<ISalesModelEvent>();
      Assert.IsNull(converter, "Unsupported message Type should not be found");
    }

    [TestMethod]
    public void SupportedEventType()
    {
      var messageResolver = serviceProvider.GetService<IMessageTypeResolver>();
      var converter = messageResolver.GetConverter<IAssetEvent>();
      Assert.IsNotNull(converter, "Unable to locate IAssetEvent converter");

      var createAssetEvent = new CreateAssetEvent() { AssetUID = Guid.NewGuid() };
      var createAssetEventSer = JsonConvert.SerializeObject(new { CreateAssetEvent = createAssetEvent });
      var deserializedObject = JsonConvert.DeserializeObject<IAssetEvent>(createAssetEventSer, converter);
      Assert.IsNotNull(deserializedObject);
    }

    [TestMethod]
    public void SupportedSharedEventType()
    {
      var messageResolver = serviceProvider.GetService<IMessageTypeResolver>();
      var converter = messageResolver.GetConverter<ICustomerEvent>();
      Assert.IsNotNull(converter, "Unable to locate ICustomerEvent converter");

      var associateCustomerUserEvent = new AssociateCustomerUserEvent() { CustomerUID = Guid.NewGuid() };
      var associateCustomerUserEventSer = JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = associateCustomerUserEvent });
      var deserializedObject = JsonConvert.DeserializeObject<ICustomerEvent>(associateCustomerUserEventSer, converter);
      Assert.IsNotNull(deserializedObject);
    }

    [TestMethod]
    public void SupportedCustomerAssetEventType()
    {
      var messageResolver = serviceProvider.GetService<IMessageTypeResolver>();
      var converter = messageResolver.GetConverter<ICustomerEvent>();
      Assert.IsNotNull(converter, "Unable to locate ICustomerEvent converter");

      var associateCustomerAssetEvent = new AssociateCustomerAssetEvent() { CustomerUID = Guid.NewGuid() };
      var associateCustomerAssetEventSer = JsonConvert.SerializeObject(new { AssociateCustomerAssetEvent = associateCustomerAssetEvent });
      var deserializedObject = JsonConvert.DeserializeObject<ICustomerEvent>(associateCustomerAssetEventSer, converter);
      Assert.IsNull(deserializedObject);
    }

    [TestMethod]
    public void UnsupportedEventType()
    {
      var messageResolver = serviceProvider.GetService<IMessageTypeResolver>();
      var converter = messageResolver.GetConverter<IGeofenceEvent>();
      Assert.IsNotNull(converter, "Unable to locate IGeofenceEvent converter");

      var favoriteGeofenceEventEvent = new FavoriteGeofenceEvent() { CustomerUID = Guid.NewGuid(), GeofenceUID = Guid.NewGuid() };
      var favoriteGeofenceEventEventSer = JsonConvert.SerializeObject(new { FavoriteGeofenceEventEvent = favoriteGeofenceEventEvent });
      var deserializedObject = JsonConvert.DeserializeObject<IGeofenceEvent>(favoriteGeofenceEventEventSer, converter);
      Assert.IsNull(deserializedObject, "unhandled event type should return null");
    }

  }
}
