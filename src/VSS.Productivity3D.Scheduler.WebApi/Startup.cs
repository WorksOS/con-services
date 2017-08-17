using System;
using System.Collections.Generic;
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

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Scheduler startup
  /// </summary>
  public class Startup
  {
    private const string _loggerRepoName = "Scheduler";
    private const int _schedulerFilterAgeDefaultDays = 28;
    IServiceCollection _serviceCollection;
    private MySqlStorage _storage = null;

    private IConfigurationStore _configStore;
    private ILogger _log;

    /// <summary>
    /// VSS.Productivity3D.Scheduler startup
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
     
      env.ConfigureLog4Net("log4net.xml", _loggerRepoName);

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
      services.AddMvc(); // for DI?
      services.AddLogging();
      
      //Configure CORS   todo needed?
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
          .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
            "X-Jwt-Assertion", "X-VisionLink-ClearCache")
          .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();

      /*** try here ***/
      var hangfireConnectionString =
        "server=localhost;port=3306;database=VSS-Productivity3D-Scheduler;userid=root;password=abc123;Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4";
      //GetConnectionString(_configStore, _log, "_SCHEDULER");
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
      /***/

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
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(_loggerRepoName);

      _serviceCollection.BuildServiceProvider(); // serviceCollection must be built BEFORE HangfireServer is added to the appBuilder

      //app.UseHangfireDashboard();
      app.UseHangfireServer();
      
      var build = _serviceCollection.BuildServiceProvider();
      _configStore = build.GetRequiredService<IConfigurationStore>();
      _log = (build.GetRequiredService<ILoggerFactory>()).CreateLogger<Startup>();

      var ageInDaysToDelete = _schedulerFilterAgeDefaultDays;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_FILTER_AGE_DAYS"), out ageInDaysToDelete))
      {
        ageInDaysToDelete = _schedulerFilterAgeDefaultDays;
        _log.LogDebug($"{ToString()}: SCHEDULER_FILTER_AGE_DAYS environment variable not available. Using default: {ageInDaysToDelete}.");
      }

      List<RecurringJobDto> recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
      _log.LogDebug($"{DateTime.Now} {this.ToString()}. PreJobsetup count of existing recurring jobs {recurringJobs.Count()}");
      //recurringJobs.ForEach(delegate(RecurringJobDto job) 
      //{
      //  RecurringJob.RemoveIfExists(job.Id);
      //});

      var LoggingTestJob = "LoggingTestJob";
      var FilterCleanupJob = "FilterCleanupJob";

      // the Filter DB environment variables will come with the 3dp/FilterService configuration
      string filterDbConnectionString = GetConnectionString(_configStore, _log, "");
      try
      {
        RecurringJob.RemoveIfExists(LoggingTestJob);
        RecurringJob.AddOrUpdate(LoggingTestJob, () => SomeJob(/*_log,*/), Cron.Minutely);
      }
      catch (Exception ex)
      {
        //todo serviceException?
        _log.LogError($"{DateTime.Now} {this.ToString()}. Unable to schedule recurring job SomeJob {ex.Message}");
        throw;
      }

      try
      {
        // todo should we remove old, or just not Upsert new? (thinking something may be wrong with old job)
        RecurringJob.RemoveIfExists(FilterCleanupJob);
        RecurringJob.AddOrUpdate(FilterCleanupJob, () => DatabaseCleanupJob(/*_log,*/ filterDbConnectionString, ageInDaysToDelete), Cron.Minutely);
      }
      catch (Exception ex)
      {
        //todo serviceException?
        _log.LogError($"{DateTime.Now} {this.ToString()}. Unable to schedule recurring job DatabaseCleanup {ex.Message}");
        throw;
      }

      recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
      _log.LogDebug($"{DateTime.Now} {this.ToString()}. PostJobSetup count of existing recurring jobs {recurringJobs.Count()}");

      if (recurringJobs == null || recurringJobs.Count < 2)
      {
        _log.LogError($"{DateTime.Now} {this.ToString()}. Incomplete list of recurring jobs {recurringJobs.Count}");
        throw new Exception("Incorrect # jobs");
      }
    }

    public void SomeJob(/*[FromServices] ILogger log,*/ )
    {
      //_log.LogError($"{DateTime.Now} {this.ToString()}: Recurring SomeJob completed successfully!");
      Console.WriteLine($"{DateTime.Now} {this.ToString()}: Recurring SomeJob completed successfully!");
    }

    public void DatabaseCleanupJob(/*[FromServices] ILogger log,*/ string filterDbConnectionString, int ageInDaysToDelete)
    {
      var cutoffActionUtcToDelete = DateTime.UtcNow.AddDays(-ageInDaysToDelete).ToString("yyyy-MM-dd HH:mm:ss"); // mySql requires this format
      Console.WriteLine($"{DateTime.Now} {this.ToString()}.DatabaseCleanupJob(): cutoffActionUtcToDelete: {cutoffActionUtcToDelete}");

      MySqlConnection dbConnection;
      try
      {
        dbConnection =new MySqlConnection(filterDbConnectionString);
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        //todo serviceException?
        Console.WriteLine($"{DateTime.Now} {this.ToString()}.DatabaseCleanupJob(): open filter DB exeception {ex.Message}");
        throw;
      }

      // todo create and use  Repo.DeleteFilterEvents(permanent)
      var empty = "\"";
      string delete = $"DELETE FROM Filter WHERE Name = {empty}{empty} AND LastActionedUTC < {empty}{cutoffActionUtcToDelete}{empty}";

      int deletedCount = 0;
      try
      {
        deletedCount = dbConnection.Execute(delete, cutoffActionUtcToDelete);
      }
      catch (Exception ex)
      {
        //todo serviceException?
        Console.WriteLine($"{DateTime.Now} {this.ToString()}.DatabaseCleanupJob(): execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close(); // todo exception?
      }

      Console.WriteLine($"{DateTime.Now} {this.ToString()}: cutoffActionUtcToDelete: {cutoffActionUtcToDelete} deletedCount: {deletedCount}");
    }

    #region private
    private string GetConnectionString([FromServices] IConfigurationStore configStore, [FromServices] ILogger log, string connectionType)
    {
      string serverName = configStore.GetValueString("MYSQL_SERVER_NAME" + connectionType);
      string serverPort = configStore.GetValueString("MYSQL_PORT" + connectionType);
      string serverDatabaseName = configStore.GetValueString("MYSQL_DATABASE_NAME" + connectionType);
      string serverUserName = configStore.GetValueString("MYSQL_USERNAME" + connectionType);
      string serverPassword = configStore.GetValueString("MYSQL_ROOT_PASSWORD" + connectionType);

      if (serverName == null || serverPort == null || serverDatabaseName == null || serverUserName == null ||
          serverPassword == null)
      {
        var errorString =
          $"Your application is attempting to use the {connectionType} connectionType but is missing an environment variable. serverName {serverName} serverPort {serverPort} serverDatabaseName {serverDatabaseName} serverUserName {serverUserName} serverPassword {serverPassword}";
        log.LogError(errorString);

        throw new InvalidOperationException(errorString);
      }

      var connString =
        "server=" + serverName +
        ";port=" + serverPort +
        ";database=" + serverDatabaseName +
        ";userid=" + serverUserName +
        ";password=" + serverPassword +
        ";Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4";
      log.LogTrace($"Connection string {connString} for connectionType: {connectionType}");

      return connString;
    }

    //private MySqlConnection OpenDatabaseConnection([FromServices] IConfigurationStore configStore, [FromServices] ILogger<Startup> log, string connectionType)
    //{
    //  // todo exception?
    //  var connection = new MySqlConnection(GetConnectionString(configStore, log, connectionType));
    //  connection.Open();

    //  return connection;
    //}

    #endregion private
  }
}
