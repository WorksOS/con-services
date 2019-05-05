using System;
using System.Linq;
using System.Threading;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.AssetMgmt3D.Proxy;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients;
using VSS.Productivity3D.Push.Clients.AssetLocations;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.WebApi.Common;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;
using VSS.Productivity3D.Scheduler.WebAPI.JobRunner;
using VSS.Productivity3D.Scheduler.WebAPI.Middleware;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Productivity3D.Scheduler.Jobs.AssetWorksManagerJob;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Productivity3D.Scheduler.WebApi.JobRunner;

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Scheduler startup
  /// </summary>
  public class Startup : BaseStartup
  {

    // This method gets called by the runtime. Use this method to add services to the container
    public override string ServiceName => "Scheduler Service API";
    public override string ServiceDescription => "A service to run scheduled jobs";
    public override string ServiceVersion => "v1";

    public const string LoggerRepoName = "scheduler";
    /// <summary>
    /// THe name of this service for swagger etc.
    /// </summary>
    /// <summary>
    /// The log file name
    /// </summary>
    private MySqlStorage _storage;

    /// <summary>
    /// VSS.Productivity3D.Scheduler startup
    /// </summary>
    public Startup(IHostingEnvironment env) : base(env, LoggerRepoName)
    {
    }



    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddTransient<IExportJob, ExportJob>();
      services.AddTransient<IJob, AssetStatusJob>();
      
      services.AddTransient<IApiClient, ApiClient>();
      services.AddTransient<ITransferProxy, TransferProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, SchedulerErrorCodesProvider>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();      
      services.AddScoped<IDataOceanClient, DataOceanClient>();
      services.AddScoped<IPegasusClient, PegasusClient>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<IJobFactory, JobFactory>();
      services.AddSingleton<WebAPI.JobRunner.JobRunner>();
      services.AddSingleton<RecurringJobRunner>();
      services.AddSingleton<IJobRunner>(s => s.GetRequiredService<WebAPI.JobRunner.JobRunner>());
      services.AddSingleton<IRecurringJobRunner>(s => s.GetRequiredService<RecurringJobRunner>());
      services.AddTransient<IDevOpsNotification, SlackNotification>();
      services.AddTransient<IDefaultJobRunner, DefaultJobsManager>();
      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddPushServiceClient<IAssetStatusServerHubClient, AssetStatusServerHubClient>();
      services.AddTransient<IFleetSummaryProxy, FleetSummaryProxy>();
      services.AddTransient<IFleetAssetSummaryProxy, FleetAssetSummaryProxy>();
      services.AddTransient<IFleetAssetDetailsProxy, FleetAssetDetailsProxy>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<IAssetResolverProxy, AssetResolverProxy>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });


      // NOTE: despite the webapi definition in the yml having a wait on the scheduler db, 
      //    the webapi seems to go ahead anyways, although the db isn't up 'enough', yet for ConfigureHangfire().
      // this sleep is only required when running the full test suite with db creation and acceptance tests.
      // a localDockerContainer build seems to be 20s is ok
      // under k8s it needs 45s 
      // note the delays before running acceptanceTests in .sh files
      //      also another delay below
      var startupWaitMs = Configuration.GetValueInt("SCHEDULER_WEBAPI_STARTUP_WAIT_MS", 0);

      if (startupWaitMs > 0)
      {
        Log.LogInformation(($"Scheduler: Startup: startupWaitMs {startupWaitMs}"));
        Thread.Sleep(startupWaitMs);
      }
      else
      {
        Log.LogInformation("Scheduler: Startup: not waiting");
      }

      ConfigureHangfire(services);

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// <summary>
    /// Configures the specified application.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The env.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {

      app.UseFilterMiddleware<SchedulerAuthentication>();
      app.UseMvc();

      Log.LogInformation("Scheduler: after ConfigureHangfire");

      ConfigureHangfireUse(app);

      int expirationManagerWaitMs = 2000;

      Thread.Sleep(expirationManagerWaitMs);
      Log.LogDebug($"Scheduler: after ConfigureHangfireUse. expirationManagerWaitMs waitMs {expirationManagerWaitMs}.");

      ServiceProvider.GetRequiredService<IDefaultJobRunner>().StartDefaultJob(new RecurringJobRequest()
      {
        JobUid = AssetStatusJob.VSSJOB_UID, 
        Schedule = "* * * * *"
      });
    }

    
    protected override void StartServices(IServiceProvider serviceProvider)
    {
      base.StartServices(serviceProvider);
      // No async / await in .NET core 2.0, coming in 3.0...
      // But this will throw an exception halting start up correctly
      serviceProvider.StartPushClients().Wait();
    }

    /// <summary>
    /// Configure hangfire
    /// </summary>
    private void ConfigureHangfire(IServiceCollection services)
    {
      // hangfire has to be started up in ConfigureServices(),
      //    i.e. before DI can be setup in Configure()
      //    therefore create temp configStore to read environment variables (unfortunately this requires log stuff)

      var hangfireConnectionString = Configuration.GetConnectionString("VSPDB");
      // queuePollIntervalSeconds needs to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup
      if (!int.TryParse(Configuration.GetValueString("SCHEDULER_QUE_POLL_INTERVAL_SECONDS"),
        out var queuePollIntervalSeconds))
        queuePollIntervalSeconds = 60;
      if (!int.TryParse(Configuration.GetValueString("SCHEDULER_JOB_EXPIRATION_CHECK_HOURS"),
        out var jobExpirationCheckIntervalHours))
        jobExpirationCheckIntervalHours = 1;
      if (!int.TryParse(Configuration.GetValueString("SCHEDULER_COUNTER_AGGREGATE_MINUTES"),
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
        Log.LogCritical($"Scheduler: ConfigureHangfire: AddHangfire failed: {ex.Message}");
        throw;
      }
      GlobalJobFilters.Filters.Add(new JobFailureFilter());
    }

    /// <summary>
    /// Configure Hangfire use
    /// </summary>
    private void ConfigureHangfireUse(IApplicationBuilder app)
    {
      try
      {
        var hangfireServerName = string.Format($"vss-3dpScheduler{Guid.NewGuid()}");
        // WorkerCount will be internally set based on #cores - on prod = 10. For a single scheduled task we need a low number
        // these affect CPU usage and number of db connections
        // things more specific to each task e.g. Hangfire.AutomaticRetryAttribute.DefaultRetryAttempts are attributes on the task
        // schedulePollingIntervalSeconds may need to be low for acceptance tests of FilterSchedulerTask_WaitForCleanup?
        if (!int.TryParse(Configuration.GetValueString("SCHEDULER_WORKER_COUNT"), out var workerCount))
          workerCount = 2;
        if (!int.TryParse(Configuration.GetValueString("SCHEDULER_SCHEDULE_POLLING_INTERVAL_SECONDS"),
          out var schedulePollingIntervalSeconds))
          schedulePollingIntervalSeconds = 60;

        var options = new BackgroundJobServerOptions
        {
          ServerName = hangfireServerName,
          WorkerCount = workerCount,
          SchedulePollingInterval = TimeSpan.FromSeconds(schedulePollingIntervalSeconds),
        };
        Log.LogDebug($"Scheduler.Configure: hangfire options: {JsonConvert.SerializeObject(options)}.");

        // do we need the dashboard?
        app.UseHangfireDashboard(options: new DashboardOptions
        {
          Authorization = new[]
          {
            new HangfireAuthorizationFilter()
          }
        });

        app.UseHangfireServer(options);
        Log.LogDebug("Scheduler.Startup: after UseHangfireServer.");
      }
      catch (Exception ex)
      {
        Log.LogError(ex, $"Scheduler: ConfigureHangfireUse: UseHangfireServer failed");
        throw;
      }
    }

    private class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
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
