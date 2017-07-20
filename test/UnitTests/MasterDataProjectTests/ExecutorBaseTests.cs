using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Repositories;
using VSS.MasterDataProxies;
using VSS.MasterDataProxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ExecutorBaseTests
  {
    public IServiceProvider serviceProvider = null;
    protected string kafkaTopicName;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection      
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>(); 
      serviceProvider = serviceCollection.BuildServiceProvider();
      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       serviceProvider.GetRequiredService<IConfigurationStore>().GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }

  }
}

