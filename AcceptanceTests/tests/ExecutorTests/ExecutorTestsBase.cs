using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  public class ExecutorTestsBase
  {
    protected IServiceProvider serviceProvider;
    protected IConfigurationStore configStore;
    protected ILoggerFactory logger;
    protected IServiceExceptionHandler serviceExceptionHandler;
    protected ProjectRepository projectRepo;
    protected IRaptorProxy raptorProxy;
    protected Dictionary<string, string> customHeaders;
    protected IKafka producer;

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
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddMemoryCache();  

      serviceProvider = serviceCollection.BuildServiceProvider();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      projectRepo = serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      raptorProxy = serviceProvider.GetRequiredService<IRaptorProxy>();
      customHeaders = new Dictionary<string, string>();
      producer = serviceProvider.GetRequiredService<IKafka>();
    }

    protected bool CreateCustomerProject(string customerUid, string projectUid)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent()
      {
        CustomerUID = Guid.Parse(customerUid),
        ProjectUID = Guid.Parse(projectUid),
        ActionUTC = actionUtc
      };

      projectRepo.StoreEvent(createProjectEvent).Wait();
      var g = projectRepo.GetProject(projectUid); g.Wait();
      return (g.Result != null ? true : false);
    }

    protected bool CreateProjectSettings(string projectUid, string settings)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = Guid.Parse(projectUid),
        Settings = settings,
        ActionUTC = actionUtc
      };

      projectRepo.StoreEvent(createProjectSettingsEvent).Wait();
      var g = projectRepo.GetProjectSettings(projectUid); g.Wait();
      return (g.Result != null ? true : false);
    }
  }
}
