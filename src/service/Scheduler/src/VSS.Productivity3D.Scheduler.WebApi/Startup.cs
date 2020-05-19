using System;
using System.Linq;
using System.Threading;
using CCSS.CWS.Client;
using CCSS.CWS.Client.MockClients;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Proxy;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.AssetLocations;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.AssetStatusJob;
using VSS.Productivity3D.Scheduler.Jobs.ExportJob;
using VSS.Productivity3D.Scheduler.Jobs.SendEmailJob;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Productivity3D.Scheduler.WebApi.JobRunner;
using VSS.Productivity3D.Scheduler.WebAPI;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;
using VSS.Productivity3D.Scheduler.WebAPI.JobRunner;
using VSS.Productivity3D.Scheduler.WebAPI.Metrics;
using VSS.Productivity3D.Scheduler.WebAPI.Middleware;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public partial class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Scheduler Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to run scheduled jobs";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <summary>
    /// THe name of this service for swagger etc.
    /// </summary>
    private MySqlStorage _storage;

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

      // Required for TIDAuthentication  
      // CCSSSCON-216 temporary move to real endpoints when available
      services.AddCwsClient<ICwsAccountClient, CwsAccountClient, MockCwsAccountClient>(CwsClientMockExtensionMethods.MOCK_ACCOUNT_KEY);

      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, SchedulerErrorCodesProvider>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddSingleton<IDataOceanClient, DataOceanClient>();
      services.AddScoped<IPegasusClient, PegasusClient>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<IJobFactory, JobFactory>();
      services.AddSingleton<WebAPI.JobRunner.JobRunner>();
      services.AddSingleton<RecurringJobRunner>();
      services.AddSingleton<IJobRunner>(s => s.GetRequiredService<WebAPI.JobRunner.JobRunner>());
      services.AddSingleton<IRecurringJobRunner>(s => s.GetRequiredService<RecurringJobRunner>());
      services.AddTransient<IDefaultJobRunner, DefaultJobsManager>();
      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddPushServiceClient<IAssetStatusServerHubClient, AssetStatusServerHubClient>();
      services.AddTransient<IFleetSummaryProxy, FleetSummaryProxy>();
      services.AddTransient<IFleetAssetSummaryProxy, FleetAssetSummaryProxy>();
      services.AddTransient<IFleetAssetDetailsProxy, FleetAssetDetailsProxy>();
      services.AddTransient<IProjectProxy, ProjectV6Proxy>();
      services.AddTransient<IFilterServiceProxy, FilterV1Proxy>();
      services.AddTransient<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddTransient<ITpaasEmailProxy, TpaasEmailProxy>();
      services.AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>();
      services.AddSingleton<IJobRegistrationManager, JobRegistrationManager>();
      services.AddSingleton<IHangfireMetricScheduler, HangfireMetricScheduler>();
      services.AddTransient<IExportEmailGenerator, ExportEmailGenerator>();

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
    //    Thread.Sleep(startupWaitMs);
      }
      else
      {
        Log.LogInformation("Scheduler: Startup: not waiting");
      }

      ConfigureHangfire(services);
    }

    /// <summary>
    /// Called by the runtime to configure the specified application and HTTP request pipeline.
    /// </summary>
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseFilterMiddleware<SchedulerAuthentication>();
      app.UseMvc();

      Log.LogInformation("Scheduler: after ConfigureHangfire");

      ConfigureHangfireUse(app);

      const int expirationManagerWaitMs = 2000;

      Thread.Sleep(expirationManagerWaitMs);
      Log.LogDebug($"Scheduler: after ConfigureHangfireUse. expirationManagerWaitMs waitMs {expirationManagerWaitMs}.");

      SetupScheduledJobs();
    }

    private void SetupScheduledJobs()
    {
      Log.LogInformation("Setting up scheduled jobs...");

      // Disabling AssetStatusJob until we are using 'dot on the map'. Until then it's redundant.
      //ServiceProvider.GetRequiredService<IDefaultJobRunner>()
      //               .StartDefaultJob(new RecurringJobRequest
      //               {
      //                 JobUid = AssetStatusJob.VSSJOB_UID,
      //                 Schedule = "* * * * *"
      //               });

      var config = Configuration.GetValueString("MACHINE_EXPORT", "");
      var schConfig = new ScheduleConfig();

      if (!string.IsNullOrEmpty(config))
      {
        try
        {
          schConfig = JsonConvert.DeserializeObject<ScheduleConfig>(config);
        }
        catch (Exception ex)
        {
          Log.LogError(ex, "Configuration for MachinePasses is invalid");
        }

        ServiceProvider.GetRequiredService<IDefaultJobRunner>()
                       .StartDefaultJob(new RecurringJobRequest
                       {
                         JobUid = Guid.Parse("39d6c48a-cc74-42d3-a839-1a6b77e8e076"),
                         Schedule = schConfig.Schedule,
                         SetupParameters = schConfig.CustomerUid,
                         RunParameters = schConfig.Emails
                       });
      }
    }

    /// <inheritdoc />
    protected override void StartServices(IServiceProvider serviceProvider)
    {
      base.StartServices(serviceProvider);
      // No async / await in .NET core 2.0, coming in 3.0...
      // But this will throw an exception halting start up correctly
      serviceProvider.StartPushClients().Wait();
    }

    /// <summary>
    /// Returns a configuration value as an int or a default value if the read fails.
    /// </summary>
    private int GetValueString(string keyName, int defaultValue)
    {
      return int.TryParse(Configuration.GetValueString(keyName), out var queuePollIntervalSeconds)
        ? queuePollIntervalSeconds
        : defaultValue;
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

      var queuePollIntervalSeconds = GetValueString("SCHEDULER_QUEUE_POLL_INTERVAL_SECONDS", 2);
      var jobExpirationCheckIntervalHours = GetValueString("SCHEDULER_JOB_EXPIRATION_CHECK_HOURS", 24);
      var countersAggregateIntervalMinutes = GetValueString("SCHEDULER_COUNTER_AGGREGATE_MINUTES", 55);

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
        // General Job processing settings e.g.
        //    WorkerCount will be internally set based on #cores.
        //       This affects CPU usage and number of db connections
        //    BackgroundJobServerOptions.SchedulePollingInterval and MySqlStorage.QueuePollInterval
        //       Affects how quickly a queued job will be picked up and processing begun
        // Individual task settings can be set as attributes on the task
        //       e.g. Hangfire.AutomaticRetryAttribute.DefaultRetryAttempts 

        var hangfireServerName = string.Format($"vss-3dpScheduler{Guid.NewGuid()}");
        if (!int.TryParse(Configuration.GetValueString("SCHEDULER_WORKER_COUNT"), out var workerCount))
        {
          workerCount = Environment.ProcessorCount * 5;
        }

        var schedulePollingIntervalSeconds = GetValueString("SCHEDULER_SCHEDULE_POLLING_INTERVAL_SECONDS", 2);

        var registrationManager = app.ApplicationServices.GetRequiredService<IJobRegistrationManager>();
        var queues = registrationManager.ResolveVssJobs().Values.Select(item => registrationManager.GetQueueName(item)).Prepend(RecurringJobRunner.QUEUE_NAME).ToArray();

        Log.LogInformation($"Available job queues: {queues.Aggregate((i, j) => i + ';' + j)}");

        var options = new BackgroundJobServerOptions
        {
          ServerName = hangfireServerName,
          WorkerCount = workerCount,
          SchedulePollingInterval = TimeSpan.FromSeconds(schedulePollingIntervalSeconds),
          Queues = queues
        };
        Log.LogDebug($"Scheduler.Configure: hangfire options: {JsonConvert.SerializeObject(options)}.");

        // Launch metrics
        app.ApplicationServices.GetRequiredService<IHangfireMetricScheduler>().Start();

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
        Log.LogError(ex, "Scheduler: ConfigureHangfireUse: UseHangfireServer failed");
        throw;
      }
    }
  }
}
