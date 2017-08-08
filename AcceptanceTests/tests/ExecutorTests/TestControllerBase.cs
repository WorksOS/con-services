using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace ExecutorTests
{
  public class TestControllerBase
  {
    protected IServiceProvider serviceProvider;
    protected IConfigurationStore configStore;
    protected ILoggerFactory logger;
    protected IServiceExceptionHandler serviceExceptionHandler;
    protected FilterRepository filterRepo;
    protected IProjectListProxy projectListProxy;
    protected IRaptorProxy raptorProxy;
    protected IKafka producer;
    protected string kafkaTopicName;

    public void SetupDI()
    {
      const string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
          .AddTransient<IProjectListProxy, ProjectListProxy>()
          .AddTransient<IRaptorProxy, RaptorProxy>()
          .AddSingleton<IKafka, RdKafkaDriver>()
          .AddTransient<IErrorCodesProvider, ErrorCodesProvider>()
          .AddMemoryCache() 
        .BuildServiceProvider();

      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      filterRepo = serviceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      projectListProxy = serviceProvider.GetRequiredService<IProjectListProxy>();
      raptorProxy = serviceProvider.GetRequiredService<IRaptorProxy>();
      producer = serviceProvider.GetRequiredService<IKafka>();
      if (!producer.IsInitializedProducer)
        producer.InitProducer(configStore);
      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IFilterEvent" +
                       configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      Assert.IsNotNull(serviceProvider.GetService<ILoggerFactory>());
    }
  }
}
 