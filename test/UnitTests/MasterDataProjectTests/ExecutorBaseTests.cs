using System;
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
    public static IServiceProvider ServiceProvider;
    protected string KafkaTopicName;
    private readonly string loggerRepoName = "UnitTestLogTest";
    protected IErrorCodesProvider ProjectErrorCodesProvider;
    protected IServiceExceptionHandler ServiceExceptionHandler;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");
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

      ServiceProvider = serviceCollection.BuildServiceProvider();
      KafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       ServiceProvider.GetRequiredService<IConfigurationStore>().GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      ProjectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
    }
  }
}