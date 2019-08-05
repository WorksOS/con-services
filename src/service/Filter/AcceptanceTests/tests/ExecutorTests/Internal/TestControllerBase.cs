using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.AssetMgmt3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Project.Repository;

namespace ExecutorTests.Internal
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    protected IConfigurationStore ConfigStore;
    protected ILoggerFactory Logger;
    protected IServiceExceptionHandler ServiceExceptionHandler;
    protected IProjectProxy ProjectProxy;
    protected IFileImportProxy FileImportProxy;
    protected IRaptorProxy RaptorProxy;
    protected IKafka Producer;
    protected string KafkaTopicName;
    protected FilterRepository FilterRepo;
    protected ProjectRepository ProjectRepo;
    protected GeofenceRepository GeofenceRepo;
    protected IGeofenceProxy GeofenceProxy;
    protected IUnifiedProductivityProxy UnifiedProductivityProxy;
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
        .AddServiceDiscovery()
        .AddTransient<IAssetResolverProxy, AssetResolverProxy>()  
        .AddTransient<IWebRequest, GracefulWebRequest>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .AddSingleton<IGeofenceProxy, GeofenceProxy>()
        .AddServiceDiscovery()
        .AddSingleton<IProjectProxy, ProjectV4ServiceDiscoveryProxy>()
        .AddSingleton<IUnifiedProductivityProxy, UnifiedProductivityProxy>()       
        .AddSingleton<IFileImportProxy, FileImportV4ServiceDiscoveryProxy>()
        .BuildServiceProvider();

      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      FilterRepo = ServiceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      ProjectRepo = ServiceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      GeofenceRepo = ServiceProvider.GetRequiredService<IRepository<IGeofenceEvent>>() as GeofenceRepository;
      ProjectProxy = ServiceProvider.GetRequiredService<IProjectProxy>();
      FileImportProxy = ServiceProvider.GetRequiredService<IFileImportProxy>();
      RaptorProxy = ServiceProvider.GetRequiredService<IRaptorProxy>();
      GeofenceProxy = ServiceProvider.GetRequiredService<IGeofenceProxy>();
      UnifiedProductivityProxy = ServiceProvider.GetRequiredService<IUnifiedProductivityProxy>();

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
