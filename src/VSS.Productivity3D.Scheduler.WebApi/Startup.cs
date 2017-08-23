using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Hangfire;
using Hangfire.MySql;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.Scheduler.Common.Utilities;


namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Scheduler startup
  /// </summary>
  public class Startup
  {
    private const string _loggerRepoName = "Scheduler";
    private const int _schedulerFilterAgeDefaultDays = 28;
    private MySqlStorage _storage = null;
    private ILogger _log = null;
    private IConfigurationStore _configStore = null;

    /// <summary>
    /// VSS.Productivity3D.Scheduler startup
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      Console.WriteLine("Startup");

      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
      Configuration = builder.Build();

      env.ConfigureLog4Net("log4net.xml", _loggerRepoName);

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
      _log.LogDebug("ConfigureServices:");

      services.AddMvc();

      var hangfireConnectionString = ConnectionUtils.GetConnectionString(_configStore, _log, "_Scheduler");
      _log.LogDebug($"ConfigureServices: Scheduler database string: {hangfireConnectionString}.");
      _storage = new MySqlStorage(hangfireConnectionString,
        new MySqlStorageOptions
        {
          QueuePollInterval = TimeSpan.FromSeconds(15),
          JobExpirationCheckInterval = TimeSpan.FromHours(1),
          CountersAggregateInterval = TimeSpan.FromMinutes(5),
          PrepareSchemaIfNecessary = true,
          DashboardJobListLimit = 50000,
          TransactionTimeout = TimeSpan.FromMinutes(1)
        });

      try
      {
        services.AddHangfire(x => x.UseStorage(_storage));
      }
      catch (Exception ex)
      {
        _log.LogError($"ConfigureServices: AddHangfire failed: {ex.Message}");
        throw new Exception($"ConfigureServices: AddHangfire failed: {ex.Message}");
      }

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
      _log.LogDebug("Configure:");
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(_loggerRepoName);

      try
      {
        app.UseHangfireServer();
      }
      catch (Exception ex)
      {
        _log.LogError($"Configure: UseHangfireServer failed: {ex.Message}");
        throw new Exception($"Configure: UseHangfireServer failed: {ex.Message}");
      }

      var ageInDaysToDelete = _schedulerFilterAgeDefaultDays;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_FILTER_AGE_DAYS"), out ageInDaysToDelete))
      {
        ageInDaysToDelete = _schedulerFilterAgeDefaultDays;
        _log.LogDebug(
          $"Configure: SCHEDULER_FILTER_AGE_DAYS environment variable not available. Using default: {ageInDaysToDelete}.");
      }

      try
      {
        List<RecurringJobDto> recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
        _log.LogDebug($"Configure: PreJobsetup count of existing recurring jobs to be deleted {recurringJobs.Count()}");
        recurringJobs.ForEach(delegate(RecurringJobDto job)
        {
          RecurringJob.RemoveIfExists(job.Id);
        });
      }
      catch (Exception ex)
      {
        _log.LogError($"Configure: Unable to cleanup existing jobs: {ex.Message}");
        throw new Exception("Configure: Unable to cleanup existing jobs");
      }

      var LoggingTestJob = "LoggingTestJob";
      var FilterCleanupJob = "FilterCleanupJob";

      // the Filter DB environment variables will come with the 3dp/FilterService configuration
      string filterDbConnectionString = _configStore.GetConnectionString("VSPDB");
      try
      {
        // todo after testing setup interval e.g. hourly
        RecurringJob.AddOrUpdate(LoggingTestJob, () => SomeJob(), Cron.MinuteInterval(2));
      }
      catch (Exception ex)
      {
        _log.LogError($"Configure: Unable to schedule recurring job: SomeJob {ex.Message}");
        throw new Exception("Configure: Unable to schedule recurring job: SomeJob");
      }

      try
      {
        // todo after testing setup interval e.g. hourly
        RecurringJob.AddOrUpdate(FilterCleanupJob,
          () => DatabaseCleanupJob(filterDbConnectionString, ageInDaysToDelete), Cron.Minutely);
      }
      catch (Exception ex)
      {
        _log.LogError($"Configure: Unable to schedule recurring job: DatabaseCleanup {ex.Message}");
        throw new Exception("Configure: Unable to schedule recurring job: DatabaseCleanup");
      }

      var recurringJobsPost = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
      _log.LogInformation($"Configure: PostJobSetup count of existing recurring jobs {recurringJobsPost.Count()}");

      if (recurringJobsPost == null || recurringJobsPost.Count < 2)
      {
        if (recurringJobsPost == null)
          _log.LogError($"Configure: Unable to get list of recurring jobs");
        else
        _log.LogError($"Configure: Incomplete list of recurring jobs {recurringJobsPost.Count}");
          
        throw new Exception("Configure: Incorrect # jobs");
      }
    }

    public void SomeJob()
    {
      var log = GetLogger();
      log.LogInformation("SomeJob: completed successfully");
    }

    public void DatabaseCleanupJob(string filterDbConnectionString, int ageInDaysToDelete)
    {
      var log = GetLogger();
      var cutoffActionUtcToDelete = DateTime.UtcNow.AddDays(-ageInDaysToDelete).ToString("yyyy-MM-dd HH:mm:ss"); // mySql requires this format
      log.LogInformation($"DatabaseCleanupJob: cutoffActionUtcToDelete: {cutoffActionUtcToDelete}");

      MySqlConnection dbConnection;
      try
      {
        dbConnection =new MySqlConnection(filterDbConnectionString);
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        log.LogError($"DatabaseCleanupJob: open filter DB exeception {ex.Message}");
        throw new Exception("DatabaseCleanupJob: open database exception");
      }

      var empty = "\"";
      string delete = $"DELETE FROM Filter WHERE Name = {empty}{empty} AND LastActionedUTC < {empty}{cutoffActionUtcToDelete}{empty}";

      int deletedCount = 0;
      try
      {
        deletedCount = dbConnection.Execute(delete, cutoffActionUtcToDelete);
      }
      catch (Exception ex)
      {
        log.LogError($"DatabaseCleanupJob: execute exeception {ex.Message}");
        throw new Exception("DatabaseCleanupJob: delete from database exception");
      }
      finally
      {
        dbConnection.Close(); 
      }

      log.LogInformation($"DatabaseCleanupJob: completed successfully. CutoffActionUtcToDelete: {cutoffActionUtcToDelete} deletedCount: {deletedCount}");
    }

    private ILogger GetLogger()
    {
      var log = GetLoggerFactory().CreateLogger<Startup>();
      return log;
    }

    private ILoggerFactory GetLoggerFactory()
    {
      const string loggerRepoName = _loggerRepoName;
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);
      return loggerFactory;
    }

  }
}
