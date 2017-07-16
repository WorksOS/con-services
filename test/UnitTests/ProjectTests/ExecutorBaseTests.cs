using System;
using KafkaConsumer.Kafka;
using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.GenericConfiguration;
using VSS.Productivity3D.MasterDataProxies;
using VSS.Productivity3D.MasterDataProxies.Interfaces;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.Repo;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace ProjectTests
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
        .AddSingleton<IConfigurationStore, VSS.GenericConfiguration.GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>(); 
      serviceProvider = serviceCollection.BuildServiceProvider();
      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       serviceProvider.GetRequiredService<IConfigurationStore>().GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }

  }
}

