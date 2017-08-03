using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumer.Tests
{

  [TestClass]
  public class LoggingTests
  {

    IServiceProvider serviceProvider = null;

    [TestMethod]
    public void CanUseLog4net()
    {
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();

      var logFileFullPath = string.Format(string.Format("{0}/{1}.log", logPath, loggerRepoName));
      if (File.Exists(logFileFullPath))
      {
        File.WriteAllText(logFileFullPath, string.Empty);
      }

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);
      ILoggerFactory loggerFactory = new LoggerFactory();     
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);
     
      // put logger into DI
      serviceProvider = new ServiceCollection()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()      
        .AddSingleton<ILoggerFactory>(loggerFactory)
        .BuildServiceProvider();

      // 1) this test is logger from outside of DI
      ILogger loggerPre = loggerFactory.CreateLogger<LoggingTests>();
      loggerPre.LogDebug("This test is outside of Container. Should reference LoggingTests.");
      Assert.IsTrue(File.Exists(logFileFullPath));

      // 2) this test is sourced from of DI
      var retrievedloggerFactory = serviceProvider.GetService<ILoggerFactory>();
      Assert.IsNotNull(retrievedloggerFactory);

      ILogger loggerPost = retrievedloggerFactory.CreateLogger<MessageResolver>();
      Assert.IsNotNull(retrievedloggerFactory);
      loggerPost.LogDebug("This test is retrieved from Container. Should reference MessageResolver.");

      System.IO.FileStream fs = new System.IO.FileStream(logFileFullPath, System.IO.FileMode.Open,
          System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
      System.IO.StreamReader sr = new System.IO.StreamReader(fs);
      List<string> allLines = new List<string>();
      while (!sr.EndOfStream)
        allLines.Add(sr.ReadLine());

      Assert.AreEqual(2, allLines.Count());
      Assert.AreEqual(2, Regex.Matches(allLines[0], "LoggingTests").Count);
      Assert.AreEqual(2, Regex.Matches(allLines[1], "MessageResolver").Count);
      fs.Dispose();
      sr.Dispose();
    }


    
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

      var ex = Assert.ThrowsException<System.InvalidOperationException>(() => serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>());
      Assert.AreEqual(ex.Message, "Unable to resolve service for type \'Microsoft.Extensions.Logging.ILoggerFactory\' while attempting to activate \'VSS.ConfigurationStore.GenericConfiguration\'.");
    }

    [TestMethod]
    public void ConstructLoggerNameFromKafkaTopic()
    {
      string[] kafkaTopic = new string[] { "VSS.Interfaces.Events.MasterData.ICustomerEvent", "VSS.Interfaces.Events.MasterData.IAssetEvent" };

      string eventType = kafkaTopic[0].Split('.').Last();
      string loggerRepoName = "MDC " + eventType;
      Assert.AreEqual("MDC ICustomerEvent", loggerRepoName, "loggerName incorrect");
    }

    private void CreateCollection(bool withLogging)
    {
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      IServiceCollection serviceCollection = new ServiceCollection()
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
          .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddSingleton<IConfigurationStore, GenericConfiguration>();

      if (withLogging)
        serviceCollection
            .AddLogging()
            .AddSingleton<ILoggerFactory>(loggerFactory);

      serviceProvider = serviceCollection
          .BuildServiceProvider();
    }


  }
}
