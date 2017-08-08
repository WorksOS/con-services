using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ExecutorBaseTests
  {
    public IServiceProvider serviceProvider;
    protected string kafkaTopicName;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      const string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddTransient<ICustomerProxy, CustomerProxy>()
        .AddTransient<IProjectListProxy, ProjectListProxy>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>()
        .AddTransient<IRepository<IFilterEvent>, FilterRepository>();

      serviceProvider = serviceCollection.BuildServiceProvider();
      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       serviceProvider.GetRequiredService<IConfigurationStore>().GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }
  }
}
