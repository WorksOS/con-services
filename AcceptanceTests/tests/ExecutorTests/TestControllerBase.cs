using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
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
    protected IRaptorProxy raptorProxy;
    protected IKafka producer;

    public void SetupLogging()
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
          .AddTransient<IRaptorProxy, RaptorProxy>()
          .AddSingleton<IKafka, RdKafkaDriver>()
          .AddMemoryCache() 
        .BuildServiceProvider();

      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      filterRepo = serviceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      raptorProxy = serviceProvider.GetRequiredService<IRaptorProxy>();
      producer = serviceProvider.GetRequiredService<IKafka>();
      Assert.IsNotNull(serviceProvider.GetService<ILoggerFactory>());
  
    }
  }
}
 