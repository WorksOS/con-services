using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.AssetMgmt3D.Proxy;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Project.Repository;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Serilog.Extensions;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;
using VSS.Productivity3D.Filter.Repository;

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
    protected IProductivity3dV2ProxyNotification Productivity3dV2ProxyNotification;
    protected IProductivity3dV2ProxyCompaction Productivity3dV2ProxyCompaction;
    protected FilterRepository FilterRepo;
    protected ProjectRepository ProjectRepo;
    protected GeofenceRepository GeofenceRepo;
    //protected IGeofenceProxy GeofenceProxy;
    //protected IUnifiedProductivityProxy UnifiedProductivityProxy;

    public void SetupDI()
    {
      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Filter.ExecutorTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()

        // for serviceDiscovery
        .AddServiceDiscovery()
        .AddTransient<IWebRequest, GracefulWebRequest>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()

        .AddTransient<IAssetResolverProxy, AssetResolverProxy>()  
        .AddTransient<IWebRequest, GracefulWebRequest>()
        .AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>()
        .AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>()
        .AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        //.AddSingleton<IGeofenceProxy, GeofenceProxy>()
        //.AddSingleton<IUnifiedProductivityProxy, UnifiedProductivityProxy>()  
        .AddSingleton<IProjectProxy, ProjectV6Proxy>()   
        .AddSingleton<IFileImportProxy, FileImportV6Proxy>()
        .BuildServiceProvider();

      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      FilterRepo = ServiceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      ProjectRepo = ServiceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      GeofenceRepo = ServiceProvider.GetRequiredService<IRepository<IGeofenceEvent>>() as GeofenceRepository;
      ProjectProxy = ServiceProvider.GetRequiredService<IProjectProxy>();
      FileImportProxy = ServiceProvider.GetRequiredService<IFileImportProxy>();
      Productivity3dV2ProxyNotification = ServiceProvider.GetRequiredService<IProductivity3dV2ProxyNotification>();
      Productivity3dV2ProxyCompaction = ServiceProvider.GetRequiredService<IProductivity3dV2ProxyCompaction>();
      //GeofenceProxy = ServiceProvider.GetRequiredService<IGeofenceProxy>();
      //UnifiedProductivityProxy = ServiceProvider.GetRequiredService<IUnifiedProductivityProxy>();

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
