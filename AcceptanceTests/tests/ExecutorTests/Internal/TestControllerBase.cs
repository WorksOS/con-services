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
using VSS.MasterData.Models.ResultHandling;
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
    protected IRaptorProxy RaptorProxy;
    protected IKafka Producer;
    protected string KafkaTopicName;
    protected FilterRepository FilterRepo;
    protected ProjectRepository ProjectRepo;
    protected GeofenceRepository GeofenceRepo;

    public void SetupDI()
    {
      const string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();

      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
          .AddTransient<IProjectListProxy, ProjectListProxy>()
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