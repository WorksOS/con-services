using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace RepositoryTests
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    protected IConfigurationStore ConfigStore;
    protected FilterRepository FilterRepo;
    protected ProjectRepository ProjectRepo;
    protected GeofenceRepository GeofenceRepo;

    public void SetupLoggingAndRepos()
    {
      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Filter.Repository.Tests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .BuildServiceProvider();

      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      FilterRepo = ServiceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      ProjectRepo = ServiceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      GeofenceRepo = ServiceProvider.GetRequiredService<IRepository<IGeofenceEvent>>() as GeofenceRepository;
      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }

    protected void WriteEventToDb(IProjectEvent projectEvent, string errorMessage)
    {
      var task = ProjectRepo.StoreEvent(projectEvent);
      task.Wait();

      Assert.AreEqual(1, task.Result, errorMessage);
    }

    protected void WriteEventToDb(IGeofenceEvent geofenceEvent, string errorMessage)
    {
      var task = GeofenceRepo.StoreEvent(geofenceEvent);
      task.Wait();

      Assert.AreEqual(1, task.Result, errorMessage);
    }

    protected void WriteEventToDb(IFilterEvent filterEvent, string errorMessage = "Filter event not written", int returnCode = 1)
    {
      var task = FilterRepo.StoreEvent(filterEvent);
      task.Wait();

      Assert.AreEqual(returnCode, task.Result, errorMessage);
    }
  }
}
