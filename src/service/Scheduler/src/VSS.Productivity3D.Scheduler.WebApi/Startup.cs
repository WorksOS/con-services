using System;
using System.Collections.Generic;
using System.Threading;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Hangfire.Storage;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;
using VSS.Productivity3D.Scheduler.WebAPI.Middleware;

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
    public const string LoggerRepoName = "WebApi";
    private MySqlStorage _storage;
    private IServiceProvider _serviceProvider;
    IServiceCollection _serviceCollection;

    /// <summary>
    /// VSS.Productivity3D.Scheduler startup
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", LoggerRepoName);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
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
      services.AddCommon<Startup>(SERVICE_TITLE);

      // NOTE: despite the webapi definition in the yml having a wait on the scheduler db, 
      //    the webapi seems to go ahead anyways, although the db isn't up 'enough', yet for ConfigureHangfire().
      // this sleep is only required when running the full test suite with db creation and acceptance tests.
      // a localDockerContainer build seems to be 20s is ok
      // under k8s it needs 45s 
      // note the delays before running acceptanceTests in .sh files
      //      also another delay below
      var serviceProvider = services.BuildServiceProvider();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var configStore = new GenericConfiguration(logger);
/*      if (!int.TryParse(configStore.GetValueString("SCHEDULER_WEBAPI_STARTUP_WAIT_MS"), out var startupWaitMs))
        startupWaitMs = 0;
      if (startupWaitMs > 0)
      {
        Console.WriteLine($"Scheduler: Startup: startupWaitMs {startupWaitMs}");
        Thread.Sleep(startupWaitMs);
      }
      else
      {
        Console.WriteLine("Scheduler: Startup: not waiting");
      }*/

      ConfigureHangfire(services);
      Console.WriteLine("Scheduler: after ConfigureHangfire");


      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddTransient<IExportJob, ExportJob>();
      services.AddTransient<IApiClient, ApiClient>();
      services.AddTransient<ITransferProxy, TransferProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, ContractExecutionStatesEnum>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
          options.Hosting.IgnorePatterns.Add(request => request.Request.GetUri().ToString().Contains("newrelic.com"));
        });
      });
      
      services.AddJaeger(SERVICE_TITLE);
      services.AddOpenTracing();

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
      _serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      _serviceProvider = _serviceCollection.BuildServiceProvider();

      app.UseCommon(SERVICE_TITLE);

      if (Configuration["newrelic"] == "true")
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }

      app.UseFilterMiddleware<SchedulerAuthentication>();
      app.UseMvc();

      var log = loggerFactory.CreateLogger<Startup>();

      ConfigureHangfireUse(app, log);
      int expirationManagerWaitMs = 2000;
      Thread.Sleep(expirationManagerWaitMs);
      log.LogDebug($"Scheduler: after ConfigureHangfireUse. expirationManagerWaitMs waitMs {expirationManagerWaitMs}.");
      Console.WriteLine($"Scheduler: after ConfigureHangfireUse. expirationManagerWaitMs waitMs {expirationManagerWaitMs}.");

      // shouldn't need this as this projects is no longer adding recurring jobs.
      // However clean up any from prior versions for a while.
      // This also verifies that we can call Hangfire.
      try
      {
        List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
        log.LogDebug(
          $"Scheduler.Configure: PreJobsetup count of existing recurring jobs: {recurringJobs.Count}");
        recurringJobs.ForEach(delegate (RecurringJobDto job)
        {
          RecurringJob.RemoveIfExists(job.Id);
        });
      }
      catch (Exception ex)
      {
        log.LogError($"Scheduler.Configure: Unable to cleanup existing jobs: {ex.Message}");
        throw;
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
      // queuePollIntervalSeconds needs to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup
      if (!int.TryParse(configStore.GetValueString("SCHEDULER_QUE_POLL_INTERVAL_SECONDS"),
        out var queuePollIntervalSeconds))
        queuePollIntervalSeconds = 60;
      if (!int.TryParse(configStore.GetValueString("SCHEDULER_JOB_EXPIRATION_CHECK_HOURS"),
        out var jobExpirationCheckIntervalHours))
        jobExpirationCheckIntervalHours = 1;
      if (!int.TryParse(configStore.GetValueString("SCHEDULER_COUNTER_AGGREGATE_MINUTES"),
        out var countersAggregateIntervalMinutes))
        countersAggregateIntervalMinutes = 55;

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
        Console.WriteLine($"Scheduler: ConfigureHangfire: AddHangfire failed: {ex.Message}");
        throw;
      }
      //GlobalJobFilters.Filters.Add(new ExportFailureFilter());
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
        // schedulePollingIntervalSeconds may need to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup?
        if (!int.TryParse(configStore.GetValueString("SCHEDULER_WORKER_COUNT"), out var workerCount))
          workerCount = 2;
        if (!int.TryParse(configStore.GetValueString("SCHEDULER_SCHEDULE_POLLING_INTERVAL_SECONDS"),
          out var schedulePollingIntervalSeconds))
          schedulePollingIntervalSeconds = 60;

        var options = new BackgroundJobServerOptions
        {
          ServerName = hangfireServerName,
          WorkerCount = workerCount,
          SchedulePollingInterval = TimeSpan.FromSeconds(schedulePollingIntervalSeconds),
        };
        log.LogDebug($"Scheduler.Configure: hangfire options: {JsonConvert.SerializeObject(options)}.");

        // do we need the dashboard?
        app.UseHangfireDashboard(options: new DashboardOptions
        {
          Authorization = new[]
          {
            new HangfireAuthorizationFilter()
          }
        });

        app.UseHangfireServer(options);
        log.LogDebug("Scheduler.Startup: after UseHangfireServer.");
      }
      catch (Exception ex)
      {
        log.LogError($"Scheduler: ConfigureHangfireUse: UseHangfireServer failed: {ex.Message}");
        throw;
      }
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
