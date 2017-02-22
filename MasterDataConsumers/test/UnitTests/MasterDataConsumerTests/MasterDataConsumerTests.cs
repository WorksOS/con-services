﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using VSS.Project.Service.Interfaces;
using VSS.Project.Service.Utils.Kafka;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Project.Service.Repositories;
using VSS.Project.Data;
using VSS.Customer.Data;
using VSS.Project.Service.Utils;
using System;
using KafkaConsumer;
using VSS.Geofence.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using log4netExtensions;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using VSS.Asset.Data;
using VSS.Device.Data;

namespace MasterDataConsumer.Tests
{

  [TestClass]
  public class MasterDataConsumerTests
  {
    IServiceProvider serviceProvider = null;

    [TestInitialize]
    public void InitTest()
    {
      // setup Ilogger
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      //var builder = new ConfigurationBuilder()
      //          .SetBasePath(logPath)
      //          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
      //var Configuration = builder.Build();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
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


          .AddTransient<IMessageTypeResolver, MessageResolver>()
          .AddTransient<IRepositoryFactory, RepositoryFactory>()

          .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()

          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddLogging()
          .AddSingleton<ILoggerFactory>(loggerFactory)
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

  }
}
