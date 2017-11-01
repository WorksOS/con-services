using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hangfire;
using Hangfire.MySql;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;


namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Scheduler startup
  /// </summary>
  public class Startup
  {
    private const string LoggerRepoName = "Scheduler";
    private MySqlStorage _storage;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _log;
    private readonly IConfigurationStore _configStore;
    IServiceCollection _serviceCollection;

    /// <summary>
    /// VSS.Productivity3D.Scheduler startup
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      int webAPIStartupWaitMs = 45000;
      Console.WriteLine($"Scheduler.Startup: webAPIStartupWaitMs {webAPIStartupWaitMs}");

      // NOTE: despite the webapi definition in the yml having a wait on the scheduler db, 
      //    the webapi seems to go ahead anyways..
      Thread.Sleep(webAPIStartupWaitMs); 

      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
      Configuration = builder.Build();

      env.ConfigureLog4Net("log4net.xml", LoggerRepoName);

      _loggerFactory = GetLoggerFactory();
      _log = GetLogger();
      _configStore = new GenericConfiguration(GetLoggerFactory());
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public IConfigurationRoot Configuration { get; }
   
    // This method gets called by the runtime. Use this method to add services to the container
    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      _log.LogDebug("Scheduler.ConfigureServices.");

      services.AddMvc();

      var hangfireConnectionString = _configStore.GetConnectionString("VSPDB");
      _log.LogDebug($"Scheduler.ConfigureServices: Scheduler database string: {hangfireConnectionString}.");
      _storage = new MySqlStorage(hangfireConnectionString,
        new MySqlStorageOptions
        {
          QueuePollInterval = TimeSpan.FromSeconds(15),
          JobExpirationCheckInterval = TimeSpan.FromHours(1),
          CountersAggregateInterval = TimeSpan.FromMinutes(5),
          PrepareSchemaIfNecessary = false,
          DashboardJobListLimit = 50000,
          TransactionTimeout = TimeSpan.FromMinutes(1)
        });

      try
      {
        services.AddHangfire(x => x.UseStorage(_storage));
      }
      catch (Exception ex)
      {
        _log.LogError($"Scheduler.ConfigureServices: AddHangfire failed: {ex.Message}");
        throw new Exception($"ConfigureServices: AddHangfire failed: {ex.Message}");
      }

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      _serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// <summary>
    /// Configures the specified application.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The env.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      _log.LogDebug("Scheduler.Configure:");

      _serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      _serviceCollection.BuildServiceProvider();

      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(LoggerRepoName);

      try
      {
        app.UseHangfireServer();
      }
      catch (Exception ex)
      {
        _log.LogError($"Scheduler.Configure: UseHangfireServer failed: {ex.Message}");
        throw new Exception($"Scheduler.Configure: UseHangfireServer failed: {ex.Message}");
      }

      try
      {
        List<RecurringJobDto> recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
        _log.LogDebug($"Scheduler.Configure: PreJobsetup count of existing recurring jobs to be deleted {recurringJobs.Count()}");
        recurringJobs.ForEach(delegate (RecurringJobDto job)
        {
          RecurringJob.RemoveIfExists(job.Id);
        });
      }
      catch (Exception ex)
      {
        _log.LogError($"Scheduler.Configure: Unable to cleanup existing jobs: {ex.Message}");
        throw new Exception("Scheduler.Configure: Unable to cleanup existing jobs");
      }

      var expectedJobCount = 0;
      var filterCleanupTaskToRun = false;
      if (!bool.TryParse(_configStore.GetValueString("SCHEDULER_FILTER_CLEANUP_TASK_RUN"), out filterCleanupTaskToRun))
      {
        filterCleanupTaskToRun = false;
      }
      Console.WriteLine($"Scheduler.Startup: filterCleanupTaskToRun {filterCleanupTaskToRun}");
      if (filterCleanupTaskToRun)
      {
        var filterCleanupTask = new FilterCleanupTask(_configStore, _loggerFactory);
        filterCleanupTask.AddTask();
        expectedJobCount += 1;
      }

      var importedProjectFileSyncTaskToRun = false;
      if (!bool.TryParse(_configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_TASK_RUN"), out importedProjectFileSyncTaskToRun))
      {
        importedProjectFileSyncTaskToRun = false;
      }
      Console.WriteLine($"Scheduler.Startup: importedProjectFileTaskToRun {importedProjectFileSyncTaskToRun}");
      if (importedProjectFileSyncTaskToRun)
      {
        var importedProjectFileSyncTask = new ImportedProjectFileSyncTask(_configStore, _loggerFactory);
        importedProjectFileSyncTask.AddTask();
        expectedJobCount += 1;
      }

      var recurringJobsPost = JobStorage.Current.GetConnection().GetRecurringJobs();
      _log.LogInformation($"Scheduler.Configure: PostJobSetup count of existing recurring jobs {recurringJobsPost.Count()}");
      if (recurringJobsPost.Count < expectedJobCount)
      {
        _log.LogError($"Scheduler.Configure: Incomplete list of recurring jobs {recurringJobsPost.Count}");
        throw new Exception("Scheduler.Configure: Incorrect # jobs");
      }
    }

    private ILogger GetLogger()
    {
      var log = GetLoggerFactory().CreateLogger<Startup>();
      return log;
    }

    private ILoggerFactory GetLoggerFactory()
    {
      const string loggerRepoName = LoggerRepoName;
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);
      return loggerFactory;
    }

  }
}
