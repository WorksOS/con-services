using System;
using System.Collections.Generic;
using System.Threading;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.TCCFileAccess;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.Productivity3D.Scheduler.WebAPI;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Scheduler startup
  /// </summary>
  public class Startup
  {
    /// <summary>
    /// THe name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "Scheduler Service API";
    /// <summary>
    /// The log file name
    /// </summary>
    public const string LOGGER_REPO_NAME = "Scheduler";
    private MySqlStorage _storage;
    private IServiceProvider _serviceProvider;
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
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", LOGGER_REPO_NAME);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();

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
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddCommon<Startup>(SERVICE_TITLE);

      ConfigureHangfire(services);

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddTransient<IImportedFileProxy, ImportedFileProxy>();
      services.AddTransient<IExportJob, ExportJob>();
      services.AddTransient<IApiClient, ApiClient>();
      services.AddTransient<ITransferProxy, TransferProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, ErrorCodesProvider>();

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
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      _serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      _serviceProvider = _serviceCollection.BuildServiceProvider();

      app.UseCommon(SERVICE_TITLE);
      app.UseTIDAuthentication();
      app.UseMvc();

      var log = loggerFactory.CreateLogger<Startup>();
 
      ConfigureHangfireUse(app, log);  
      try
      {
        List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
        log.LogDebug(
          $"Scheduler.Configure: PreJobsetup count of existing recurring jobs to be deleted {recurringJobs.Count}");
        recurringJobs.ForEach(delegate(RecurringJobDto job)
        {
          RecurringJob.RemoveIfExists(job.Id);
        });
      }
      catch (Exception ex)
      {
        log.LogError($"Scheduler.Configure: Unable to cleanup existing jobs: {ex.Message}");
        throw;
      }

      var expectedJobCount = 0;
      expectedJobCount += ConfigureFilterCleanupTask(log, expectedJobCount);
      expectedJobCount += ConfigureSyncSurveyedSurfacesTask(log, expectedJobCount);
      expectedJobCount += ConfigureSyncOtherFilesTask(log, expectedJobCount);
  
      var recurringJobsPost = JobStorage.Current.GetConnection().GetRecurringJobs();
      if (recurringJobsPost.Count < expectedJobCount)
      {
        log.LogError($"Scheduler.Configure: Incomplete list of recurring jobs {recurringJobsPost.Count}");
        throw new Exception("Scheduler.Configure: Incorrect # jobs");
      }
    }

    /// <summary>
    /// Configure hangfire
    /// </summary>
    private void ConfigureHangfire(IServiceCollection services)
    {
      // hangfire has to be started up in ConfigureServices(),
      //    i.e. before DI can be setup in Configure()
      //    therefore create temp configStore to read environment variables (unfortunately this requires log stuff)
      var serviceProvider = services.BuildServiceProvider();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var configStore = new GenericConfiguration(logger);

      var hangfireConnectionString = configStore.GetConnectionString("VSPDB");
      int queuePollIntervalSeconds;
      int jobExpirationCheckIntervalHours;
      int countersAggregateIntervalMinutes;
      // queuePollIntervalSeconds needs to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup
      if (!int.TryParse(configStore.GetValueString("SCHEDULER_QUE_POLL_INTERVAL_SECONDS"),
        out queuePollIntervalSeconds))
        queuePollIntervalSeconds = 60;
      if (!int.TryParse(configStore.GetValueString("SCHEDULER_JOB_EXPIRATION_CHECK_HOURS"),
        out jobExpirationCheckIntervalHours))
        jobExpirationCheckIntervalHours = 1;
      if (!int.TryParse(configStore.GetValueString("SCHEDULER_COUNTER_AGGREGATE_MINUTES"),
        out countersAggregateIntervalMinutes))
        countersAggregateIntervalMinutes = 55;

      Console.WriteLine(
        $"Scheduler.ConfigureServices: Scheduler database string: {hangfireConnectionString} queuePollIntervalSeconds {queuePollIntervalSeconds} jobExpirationCheckIntervalHours {jobExpirationCheckIntervalHours} countersAggregateIntervalMinutes {countersAggregateIntervalMinutes}.");
      _storage = new MySqlStorage(hangfireConnectionString,
        new MySqlStorageOptions
        {
          QueuePollInterval = TimeSpan.FromSeconds(queuePollIntervalSeconds),
          JobExpirationCheckInterval = TimeSpan.FromHours(24),
          CountersAggregateInterval = TimeSpan.FromMinutes(countersAggregateIntervalMinutes),
          PrepareSchemaIfNecessary = false
        });

      try
      {
        services.AddHangfire(x => x.UseStorage(_storage));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Scheduler.ConfigureServices: AddHangfire failed: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Configure Hangfire use
    /// </summary>
    private void ConfigureHangfireUse(IApplicationBuilder app, ILogger<Startup> log)
    {
      var configStore = _serviceProvider.GetRequiredService<IConfigurationStore>();

      try
      {
        var hangfireServerName = string.Format($"vss-3dpScheduler{Guid.NewGuid()}");
        // WorkerCount will be internally set based on #cores - on prod = 10. For a single scheduled task we need a low number
        // these affect CPU usage and number of db connections
        // things more specific to each task e.g. Hangfire.AutomaticRetryAttribute.DefaultRetryAttempts are attributes on the task
        int workerCount; // Math.Min(Environment.ProcessorCount * 5, 20)
        // schedulePollingIntervalSeconds may need to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup?
        int schedulePollingIntervalSeconds; // DelayedJobScheduler.DefaultPollingDelay = 15 seconds
        if (!int.TryParse(configStore.GetValueString("SCHEDULER_WORKER_COUNT"), out workerCount))
          workerCount = 2;
        if (!int.TryParse(configStore.GetValueString("SCHEDULER_SCHEDULE_POLLING_INTERVAL_SECONDS"),
          out schedulePollingIntervalSeconds))
          schedulePollingIntervalSeconds = 60;

        var options = new BackgroundJobServerOptions
        {
          ServerName = hangfireServerName,
          WorkerCount = workerCount,
          SchedulePollingInterval = TimeSpan.FromSeconds(schedulePollingIntervalSeconds),
        };
        log.LogDebug($"Scheduler.Configure: hangfire options: {JsonConvert.SerializeObject(options)}.");
        app.UseHangfireDashboard(options: new DashboardOptions
        {
          Authorization = new[]
          {
            new HangfireAuthorizationFilter()
          }
        });
        app.UseHangfireServer(options);

        int expirationManagerWaitMs = 2000;
        Thread.Sleep(expirationManagerWaitMs);
        log.LogDebug($"Scheduler.Startup: after expirationManagerWaitMs wait, proceed....");
      }
      catch (Exception ex)
      {
        log.LogError($"Scheduler.Configure: UseHangfireServer failed: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Configure filter cleanup task
    /// </summary>
    private int ConfigureFilterCleanupTask(ILogger<Startup> log, int expectedJobCount)
    {
      var configStore = _serviceProvider.GetRequiredService<IConfigurationStore>();

      var filterCleanupTaskToRun = false;
      if (!bool.TryParse(configStore.GetValueString("SCHEDULER_FILTER_CLEANUP_TASK_RUN"), out filterCleanupTaskToRun))
      {
        filterCleanupTaskToRun = false;
      }
      log.LogDebug($"Scheduler.Startup: filterCleanupTaskToRun {filterCleanupTaskToRun}");
      if (filterCleanupTaskToRun)
      {
        // stagger startup of 2nd task so the initial runs don't deadlock
        if (expectedJobCount > 0)
          Thread.Sleep(2000);

        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        var filterCleanupTask = new FilterCleanupTask(configStore, loggerFactory);
        filterCleanupTask.AddTask();
        return 1;
      }
      return 0;
    }

    /// <summary>
    /// Configure surveyed surface file sync task
    /// </summary>
    private int ConfigureSyncSurveyedSurfacesTask(ILogger<Startup> log, int expectedJobCount)
    {
      var configStore = _serviceProvider.GetRequiredService<IConfigurationStore>();

      var projectFileSyncSSTaskToRun = false;
      if (!bool.TryParse(configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_SS_TASK_RUN"),
        out projectFileSyncSSTaskToRun))
      {
        projectFileSyncSSTaskToRun = false;
      }
      log.LogDebug(
        $"Scheduler.Startup: importedProjectFileTaskToRun (SurveyedSurface type only) {projectFileSyncSSTaskToRun}");
      if (projectFileSyncSSTaskToRun)
      {
        // stagger startup of 2nd task so the initial runs don't deadlock
        if (expectedJobCount > 0)
          Thread.Sleep(2000);

        var raptorProxy = _serviceProvider.GetRequiredService<IRaptorProxy>();
        var tPaasProxy = _serviceProvider.GetRequiredService<ITPaasProxy>();
        var impFileProxy = _serviceProvider.GetRequiredService<IImportedFileProxy>();
        var fileRepo = _serviceProvider.GetRequiredService<IFileRepository>();
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        var importedProjectFileSyncTask = new SurveyedSurfaceFileSyncTask(configStore, loggerFactory, raptorProxy,
          tPaasProxy, impFileProxy, fileRepo);
        importedProjectFileSyncTask.AddTask();
        return 1;
      }
      return 0;
    }

    /// <summary>
    /// Configure other imported file sync task
    /// </summary>
    private int ConfigureSyncOtherFilesTask(ILogger<Startup> log, int expectedJobCount)
    {
      var configStore = _serviceProvider.GetRequiredService<IConfigurationStore>();
 
      var projectFileSyncNonSSTaskToRun = false;
      if (!bool.TryParse(configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_NonSS_TASK_RUN"),
        out projectFileSyncNonSSTaskToRun))
      {
        projectFileSyncNonSSTaskToRun = false;
      }
      log.LogDebug(
        $"Scheduler.Startup: importedProjectFileTaskToRun (nonSurveyedSurface types only) {projectFileSyncNonSSTaskToRun}");
      if (projectFileSyncNonSSTaskToRun)
      {
        // stagger startup of 2nd task so the initial runs don't deadlock
        if (expectedJobCount > 0)
          Thread.Sleep(2000);

        var raptorProxy = _serviceProvider.GetRequiredService<IRaptorProxy>();
        var tPaasProxy = _serviceProvider.GetRequiredService<ITPaasProxy>();
        var impFileProxy = _serviceProvider.GetRequiredService<IImportedFileProxy>();
        var fileRepo = _serviceProvider.GetRequiredService<IFileRepository>();
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        var importedProjectFileSyncTask = new OtherImportedFileSyncTask(configStore, loggerFactory, raptorProxy,
          tPaasProxy, impFileProxy, fileRepo);
        importedProjectFileSyncTask.AddTask();
        return 1;
      }
      return 0;
    }

    internal class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
      private readonly string[] _roles;

      public HangfireAuthorizationFilter(params string[] roles)
      {
        _roles = roles;
      }

      public bool Authorize(DashboardContext context)
      {
        return true;
      }
    }

  }
}
