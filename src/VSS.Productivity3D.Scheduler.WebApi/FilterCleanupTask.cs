using System;
using System.Collections.Generic;
using Dapper;
using Hangfire;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Utilities;


namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// FilterCleanupTask to remove old transient filters
  /// </summary>
  public class FilterCleanupTask
  {
    private readonly ILogger _log;
    private readonly IConfigurationStore _configStore = null;
    private static int DefaultFilterAgeDefaultMinutes { get; } = 4;
    private static int DefaultTaskIntervalDefaultMinutes { get; } = 5;
  
    /// <summary>
    /// Initializes the FilterCleanupTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="loggerFactory"></param>
    public FilterCleanupTask(IConfigurationStore configStore, ILoggerFactory loggerFactory)
    {
      Console.WriteLine($"FilterCleanupTask configStore {configStore} loggerFactory: {loggerFactory}");
      _log = loggerFactory.CreateLogger<FilterCleanupTask>();
      _configStore = configStore;  
    }

    /// <summary>
    /// Add a Task to the scheduler
    /// </summary>
    public void AddTask()
    {
      var startUtc = DateTime.UtcNow;
     
      int ageInMinutesToDelete;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_FILTER_CLEANUP_TASK_AGE_MINUTES"), out ageInMinutesToDelete))
      {
        ageInMinutesToDelete = DefaultFilterAgeDefaultMinutes;
       }

      // lowest interval is minutes 
      int taskIntervalMinutes;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_FILTER_CLEANUP_TASK_INTERVAL_MINUTES"), out taskIntervalMinutes))
      {
        taskIntervalMinutes = DefaultTaskIntervalDefaultMinutes;
      }

      var FilterCleanupTask = "FilterCleanupTask";
      string filterDbConnectionString = ConnectionUtils.GetConnectionStringMySql(_configStore, _log, "_FILTER");
      _log.LogInformation($"FilterCleanupTask: ageInMinutesToDelete: {ageInMinutesToDelete} taskIntervalSeconds: {taskIntervalMinutes} filterDbConnectionString: {filterDbConnectionString}.");
      Console.WriteLine($"FilterCleanupTask: ageInMinutesToDelete: {ageInMinutesToDelete} taskIntervalSeconds: {taskIntervalMinutes} filterDbConnectionString: {filterDbConnectionString}.");

      try
      {
        RecurringJob.AddOrUpdate(FilterCleanupTask,
          () => DatabaseCleanupTask(filterDbConnectionString, ageInMinutesToDelete), Cron.MinuteInterval(taskIntervalMinutes));
      }
      catch (Exception ex)
      {
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"Unable to schedule recurring job: exception {ex.Message}") }
        };
        NewRelicUtils.NotifyNewRelic("DatabaseCleanupTask", "Fatal", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
        throw;
      }
    }

    /// <summary>
    /// cleanup transient filters over n minutes old
    /// </summary>
    /// <param name="filterDbConnectionString"></param>
    /// <param name="ageInMinutesToDelete"></param>
    public void DatabaseCleanupTask(string filterDbConnectionString, int ageInMinutesToDelete)
    {
      var startUtc = DateTime.UtcNow;
      Dictionary<string, object> newRelicAttributes;

      var cutoffActionUtcToDelete = startUtc.AddMinutes(-ageInMinutesToDelete).ToString("yyyy-MM-dd HH:mm:ss"); // mySql requires this format

      MySqlConnection dbConnection = new MySqlConnection(filterDbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"open filter DB exeception {ex.Message}") }
        };
        NewRelicUtils.NotifyNewRelic("DatabaseCleanupTask", "Fatal", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
        throw;
      }
      finally
      {
        dbConnection.Close();
      }

      var empty = "\"";
      string deleteCommand = $"DELETE FROM Filter WHERE (Name = {empty}{empty} OR Name IS NULL) AND LastActionedUTC < {empty}{cutoffActionUtcToDelete}{empty}";
      int deletedCount = 0;
      try
      {
        deletedCount = dbConnection.Execute(deleteCommand, cutoffActionUtcToDelete);
        Console.WriteLine($"FilterCleanupTask.DatabaseCleanupTask: connectionString {dbConnection.ConnectionString} deleteCommand {deleteCommand} deletedCount {deletedCount}");
      }
      catch (Exception ex)
      {
        newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"execute delete on filter DB exeception {ex.Message}") }
        };
        NewRelicUtils.NotifyNewRelic("DatabaseCleanupTask", "Fatal", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
        throw;
      }
      finally
      {
        dbConnection.Close();
      }
      
      newRelicAttributes = new Dictionary<string, object> {
        { "ageInMinutesToDelete", ageInMinutesToDelete }, {"cutoffActionUtcToDelete", cutoffActionUtcToDelete }, {"deletedCount", deletedCount}
      };
      NewRelicUtils.NotifyNewRelic("DatabaseCleanupTask",  "Information", startUtc,  (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
    }
  }
}
