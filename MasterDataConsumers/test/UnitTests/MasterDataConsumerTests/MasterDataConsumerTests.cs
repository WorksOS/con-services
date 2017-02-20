using Microsoft.VisualStudio.TestTools.UnitTesting;
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
          .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
          .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
          .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
          .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
          .AddTransient<IMessageTypeResolver, MessageResolver>()
          .AddTransient<IRepositoryFactory, RepositoryFactory>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddLogging()
          .AddSingleton<ILoggerFactory>(loggerFactory)
          .BuildServiceProvider();
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

  }
}
