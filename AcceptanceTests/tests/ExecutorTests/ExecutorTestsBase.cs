using System;
using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace ExecutorTests
{
  public class ExecutorTestsBase
  {
    protected IServiceProvider serviceProvider;
    protected IConfigurationStore configStore;
    protected ILoggerFactory logger;
    protected IServiceExceptionHandler serviceExceptionHandler;
    protected ProjectRepository projectRepo;
    
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
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>(); 
  
      serviceProvider = serviceCollection.BuildServiceProvider();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      projectRepo = serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
    }

    protected bool CreateProjectSettings(string projectUid, string settings)
    {
      // todo
      //DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      //var createProjectSettingsEvent = new CreateProjectSettingsEvent()
      //{
      //  ProjectUid = projectUid,
      //  Settings = settings,
      //  ActionUTC = actionUTC
      //};

      //projectRepo.StoreEvent(createProjectSettingsEvent).Wait();
      //var g = projectRepo.GetProjectSettings(projectUid); g.Wait();
      //return (g.Result != null ? true : false);
      return true;
    }

  }
}
