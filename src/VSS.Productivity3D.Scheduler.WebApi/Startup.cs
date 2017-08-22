using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Hangfire;
using Hangfire.Client;
using Hangfire.MySql;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
      services.AddMvc(); // needed?

      var hangfireConnectionString =
        "server=localhost;port=3306;database=VSS-Productivity3D-Scheduler;userid=root;password=abc123;Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4";
      _log.LogDebug($".ConfigureServices: Scheduler database string: {hangfireConnectionString}.");
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

      services.AddHangfire(x => x.UseStorage(_storage));
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
      app.UseHangfireServer();
    
      var ageInDaysToDelete = _schedulerFilterAgeDefaultDays;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_FILTER_AGE_DAYS"), out ageInDaysToDelete))
      {
        ageInDaysToDelete = _schedulerFilterAgeDefaultDays;
        _log.LogDebug($".Configure: SCHEDULER_FILTER_AGE_DAYS environment variable not available. Using default: {ageInDaysToDelete}.");
      }

      try
      {
        List<RecurringJobDto> recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
        _log.LogDebug($".Configure: PreJobsetup count of existing recurring jobs {recurringJobs.Count()}");
        recurringJobs.ForEach(delegate(RecurringJobDto job)
        {
          RecurringJob.RemoveIfExists(job.Id);
        });
      }
      catch (Exception ex)
      {
        _log.LogError($".Configure: Unable to cleanup existing jobs: {ex.Message}");
        throw new Exception(".Configure: Unable to cleanup existing jobs");
      }

      var LoggingTestJob = "LoggingTestJob";
      var FilterCleanupJob = "FilterCleanupJob";

      // the Filter DB environment variables will come with the 3dp/FilterService configuration
      string filterDbConnectionString = ConnectionUtils.GetConnectionString(_configStore, _log, "");
      try
      {
        // todo after testing setup interval e.g. hourly
        RecurringJob.AddOrUpdate(LoggingTestJob, () => SomeJob(), Cron.MinuteInterval(5));
      }
      catch (Exception ex)
      {
        _log.LogError($".Configure: Unable to schedule recurring job: SomeJob {ex.Message}");
        throw new Exception(".Configure: Unable to schedule recurring job: SomeJob");
      }

      try
      {
        // todo after testing setup interval e.g. hourly
        RecurringJob.AddOrUpdate(FilterCleanupJob, () => DatabaseCleanupJob(filterDbConnectionString, ageInDaysToDelete), Cron.MinuteInterval(5));
      }
      catch (Exception ex)
      {
        _log.LogError($".Configure: Unable to schedule recurring job: DatabaseCleanup {ex.Message}");
        throw new Exception(".Configure: Unable to schedule recurring job: DatabaseCleanup");
      }

      var recurringJobsPost = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
      _log.LogInformation($".Configure: PostJobSetup count of existing recurring jobs {recurringJobsPost.Count()}");

      if (recurringJobsPost == null || recurringJobsPost.Count < 2)
      {
        _log.LogError($".Configure: Incomplete list of recurring jobs {recurringJobsPost.Count}");
        throw new Exception(".Configure: Incorrect # jobs");
      }
    }

    public void SomeJob()
    {
      var log = GetLogger();
      log.LogInformation($".SomeJob(): Recurring SomeJob completed successfully!");
    }

    public void DatabaseCleanupJob(string filterDbConnectionString, int ageInDaysToDelete)
    {
      var log = GetLogger();
      var cutoffActionUtcToDelete = DateTime.UtcNow.AddDays(-ageInDaysToDelete).ToString("yyyy-MM-dd HH:mm:ss"); // mySql requires this format
      log.LogInformation($"DatabaseCleanupJob(): cutoffActionUtcToDelete: {cutoffActionUtcToDelete}");

      MySqlConnection dbConnection;
      try
      {
        dbConnection =new MySqlConnection(filterDbConnectionString);
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        log.LogError($"DatabaseCleanupJob(): open filter DB exeception {ex.Message}");
        throw new Exception(".DatabaseCleanupJob: open database exception");
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
        log.LogError($".DatabaseCleanupJob(): execute exeception {ex.Message}");
        throw new Exception(".DatabaseCleanupJob: delete from database exception");
      }
      finally
      {
        dbConnection.Close(); 
      }

      log.LogInformation($".DatabaseCleanupJob: deletedCount: {deletedCount}");
    }

    private ILogger GetLogger()
    {
      var log = GetLoggerFactory().CreateLogger<Startup>();
      return log;
    }

    private ILoggerFactory GetLoggerFactory()
    {
      const string loggerRepoName = "Scheduler"; // _loggerRepoName;
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);
      return loggerFactory;
    }

  }
}
