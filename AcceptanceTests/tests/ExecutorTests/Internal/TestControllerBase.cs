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
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace ExecutorTests.Internal
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    protected IConfigurationStore ConfigStore;
    protected ILoggerFactory Logger;
    protected IServiceExceptionHandler ServiceExceptionHandler;
    protected IProjectListProxy ProjectListProxy;
    protected IFileListProxy FileListProxy;
    protected IRaptorProxy RaptorProxy;
    protected IKafka Producer;
    protected string KafkaTopicName;
    protected FilterRepository FilterRepo;
    protected ProjectRepository ProjectRepo;
    protected GeofenceRepository GeofenceRepo;
    private const string LOGGER_REPO_NAME = "UnitTestLogTest";

    public void SetupDI()
    {
      Log4NetProvider.RepoName = LOGGER_REPO_NAME;
      Log4NetAspExtensions.ConfigureLog4Net(LOGGER_REPO_NAME, "log4nettest.xml");
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(LOGGER_REPO_NAME);

      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
          .AddTransient<IProjectListProxy, ProjectListProxy>()
          .AddTransient<IFileListProxy, FileListProxy>()
          .AddTransient<IRaptorProxy, RaptorProxy>()
          .AddSingleton<IKafka, RdKafkaDriver>()
          .AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>()
          .AddMemoryCache()
        .BuildServiceProvider();

      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      FilterRepo = ServiceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      ProjectRepo = ServiceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      GeofenceRepo = ServiceProvider.GetRequiredService<IRepository<IGeofenceEvent>>() as GeofenceRepository;
      ProjectListProxy = ServiceProvider.GetRequiredService<IProjectListProxy>();
      FileListProxy = ServiceProvider.GetRequiredService<IFileListProxy>();
      RaptorProxy = ServiceProvider.GetRequiredService<IRaptorProxy>();

      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }

    protected void WriteEventToDb(IGeofenceEvent geofenceEvent)
    {
      var task = GeofenceRepo.StoreEvent(geofenceEvent);
      task.Wait();

      Assert.AreEqual(1, task.Result, "Geofence event not written");
    }

    protected void WriteEventToDb(IProjectEvent projectEvent)
    {
      var task = ProjectRepo.StoreEvent(projectEvent);
      task.Wait();

      Assert.AreEqual(1, task.Result, "Project event not written");
    }

    protected void WriteEventToDb(IFilterEvent filterEvent)
    {
      var task = FilterRepo.StoreEvent(filterEvent);
      task.Wait();

      Assert.AreEqual(1, task.Result, "Filter event not written");
    }
  }
}
