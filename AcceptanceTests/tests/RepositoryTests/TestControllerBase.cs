using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
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
    private readonly string loggerRepoName = "UnitTestLogTest";

    public void SetupLoggingAndRepos()
    {
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      this.ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddMemoryCache()
        .BuildServiceProvider();

      this.ConfigStore = this.ServiceProvider.GetRequiredService<IConfigurationStore>();
      this.FilterRepo = this.ServiceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      this.ProjectRepo = this.ServiceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      this.GeofenceRepo = this.ServiceProvider.GetRequiredService<IRepository<IGeofenceEvent>>() as GeofenceRepository;
      Assert.IsNotNull(this.ServiceProvider.GetService<ILoggerFactory>());
    }

    protected void WriteEventToDb(IProjectEvent projectEvent, string errorMessage)
    {
      var task = this.ProjectRepo.StoreEvent(projectEvent);
      task.Wait();

      Assert.AreEqual(1, task.Result, errorMessage);
    }

    protected void WriteEventToDb(IGeofenceEvent geofenceEvent, string errorMessage)
    {
      var task = this.GeofenceRepo.StoreEvent(geofenceEvent);
      task.Wait();

      Assert.AreEqual(1, task.Result, errorMessage);
    }

    protected void WriteEventToDb(IFilterEvent filterEvent, string errorMessage = "Filter event not written", int returnCode = 1)
    {
      var task = this.FilterRepo.StoreEvent(filterEvent);
      task.Wait();

      Assert.AreEqual(returnCode, task.Result, errorMessage);
    }
  }
}