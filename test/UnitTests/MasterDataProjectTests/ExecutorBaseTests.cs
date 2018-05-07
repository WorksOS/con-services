using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;

namespace VSS.MasterData.ProjectTests
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

      Log4NetProvider.RepoName = loggerRepoName;
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddTransient<IProjectRepository, ProjectRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();

      serviceProvider = serviceCollection.BuildServiceProvider();
      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       serviceProvider.GetRequiredService<IConfigurationStore>().GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }
  }
}