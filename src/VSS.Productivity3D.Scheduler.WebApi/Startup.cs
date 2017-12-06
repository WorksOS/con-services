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
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Utilities;


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
    private readonly IRaptorProxy _raptorProxy;
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
      _raptorProxy = new RaptorProxy(_configStore, _loggerFactory);

      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
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
      int queuePollIntervalSeconds;
      int jobExpirationCheckIntervalHours;
      int countersAggregateIntervalMinutes;
      // queuePollIntervalSeconds needs to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_QUE_POLL_INTERVAL_SECONDS"), out queuePollIntervalSeconds))
        queuePollIntervalSeconds = 60;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_JOB_EXPIRATION_CHECK_HOURS"), out jobExpirationCheckIntervalHours))
        jobExpirationCheckIntervalHours = 1;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_COUNTER_AGGREGATE_MINUTES"), out countersAggregateIntervalMinutes))
        countersAggregateIntervalMinutes = 55;

      _log.LogDebug($"Scheduler.ConfigureServices: Scheduler database string: {hangfireConnectionString} queuePollIntervalSeconds {queuePollIntervalSeconds} jobExpirationCheckIntervalHours {jobExpirationCheckIntervalHours} countersAggregateIntervalMinutes {countersAggregateIntervalMinutes}.");
      _storage = new MySqlStorage(hangfireConnectionString,
        new MySqlStorageOptions
        {
          QueuePollInterval = TimeSpan.FromSeconds(queuePollIntervalSeconds), 
          JobExpirationCheckInterval = TimeSpan.FromHours(jobExpirationCheckIntervalHours),
          CountersAggregateInterval = TimeSpan.FromMinutes(countersAggregateIntervalMinutes),
          PrepareSchemaIfNecessary = false
        });

      try
      {
        services.AddHangfire(x => x.UseStorage(_storage));  
      }
      catch (Exception ex)
      {
        _log.LogError($"Scheduler.ConfigureServices: AddHangfire failed: {ex.Message}");
        throw;
      }

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
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
        var hangfireServerName = string.Format($"vss-3dpScheduler{Guid.NewGuid()}");
        // WorkerCount will be internally set based on #cores - on prod = 10. For a single scheduled task we need a low number
        // these affect CPU usage and number of db connections
        // things more specific to each task e.g. Hangfire.AutomaticRetryAttribute.DefaultRetryAttempts are attributes on the task
        int workerCount; // Math.Min(Environment.ProcessorCount * 5, 20)
        // schedulePollingIntervalSeconds may need to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup?
        int schedulePollingIntervalSeconds; // DelayedJobScheduler.DefaultPollingDelay = 15 seconds
        //int heartbeatIntervalSeconds; // hangfire ServerHeartbeat.DefaultHeartbeatInterval default = 30 secs
        //int serverCheckIntervalSeconds; // ServerWatchdog.DefaultCheckInterval
        if (!int.TryParse(_configStore.GetValueString("SCHEDULER_WORKER_COUNT"), out workerCount))
          workerCount = 2;
        if (!int.TryParse(_configStore.GetValueString("SCHEDULER_SCHEDULE_POLLING_INTERVAL_SECONDS"), out schedulePollingIntervalSeconds))
          schedulePollingIntervalSeconds = 60;
        //if (!int.TryParse(_configStore.GetValueString("SCHEDULER_HEARTBEAT_INTERVAL_SECONDS"), out heartbeatIntervalSeconds))
        //  heartbeatIntervalSeconds = 60;
        //if (!int.TryParse(_configStore.GetValueString("SCHEDULER_SERVER_CHECK_INTERVAL_SECONDS"), out serverCheckIntervalSeconds))
        //  serverCheckIntervalSeconds = 60;

        _log.LogDebug($"Scheduler.Configure: workerCount: {workerCount} schedulePollingIntervalSeconds {schedulePollingIntervalSeconds}.");

        var options = new BackgroundJobServerOptions
        {
          ServerName = hangfireServerName,
          WorkerCount = workerCount,
          SchedulePollingInterval = TimeSpan.FromSeconds(schedulePollingIntervalSeconds)
          //HeartbeatInterval = TimeSpan.FromSeconds(heartbeatIntervalSeconds),
          //ServerCheckInterval = TimeSpan.FromSeconds(serverCheckIntervalSeconds)
        };
        app.UseHangfireServer(options);

        int expirationManagerWaitMs = 2000;
        Console.WriteLine($"Scheduler.Startup: expirationManagerWaitMs {expirationManagerWaitMs}");
        Thread.Sleep(expirationManagerWaitMs);
        Console.WriteLine($"Scheduler.Startup: after expirationManagerWaitMs wait, proceed....");
      }
      catch (Exception ex)
      {
        _log.LogError($"Scheduler.Configure: UseHangfireServer failed: {ex.Message}");
        throw;
      }

      try
      {
        List<RecurringJobDto> recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
        _log.LogDebug(
          $"Scheduler.Configure: PreJobsetup count of existing recurring jobs to be deleted {recurringJobs.Count()}");
        recurringJobs.ForEach(delegate(RecurringJobDto job)
        {
          RecurringJob.RemoveIfExists(job.Id);
        });
      }
      catch (Exception ex)
      {
        _log.LogError($"Scheduler.Configure: Unable to cleanup existing jobs: {ex.Message}");
        throw;
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
      if (!bool.TryParse(_configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_TASK_RUN"),
        out importedProjectFileSyncTaskToRun))
      {
        importedProjectFileSyncTaskToRun = false;
      }
      Console.WriteLine($"Scheduler.Startup: importedProjectFileTaskToRun {importedProjectFileSyncTaskToRun}");
      if (importedProjectFileSyncTaskToRun)
      {
        // stagger startup of 2nd task so the initial runs don't deadlock
        if (filterCleanupTaskToRun)
          Thread.Sleep(2000);

        var importedProjectFileSyncTask = new ImportedProjectFileSyncTask(_configStore, _loggerFactory, _raptorProxy);
        importedProjectFileSyncTask.AddTask();
        expectedJobCount += 1;
      }

      var recurringJobsPost = JobStorage.Current.GetConnection().GetRecurringJobs();
      _log.LogInformation(
        $"Scheduler.Configure: PostJobSetup count of existing recurring jobs {recurringJobsPost.Count()}");
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
