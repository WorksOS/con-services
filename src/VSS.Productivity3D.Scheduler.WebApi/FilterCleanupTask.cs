using System;
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
      _log.LogDebug($"FilterCleanupTask.AddTask. configStore: {_configStore}");
     
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

      var FilterCleanupJob = "FilterCleanupJob";
      string filterDbConnectionString = ConnectionUtils.GetConnectionString(_configStore, _log, "_FILTER");
      _log.LogInformation($"FilterCleanupTask: ageInMinutesToDelete: {ageInMinutesToDelete} taskIntervalSeconds: {taskIntervalMinutes} filterDbConnectionString: {filterDbConnectionString}.");
      Console.WriteLine($"FilterCleanupTask: ageInMinutesToDelete: {ageInMinutesToDelete} taskIntervalSeconds: {taskIntervalMinutes} filterDbConnectionString: {filterDbConnectionString}.");

      try
      {
        RecurringJob.AddOrUpdate(FilterCleanupJob,
          () => DatabaseCleanupJob(filterDbConnectionString, ageInMinutesToDelete), Cron.MinuteInterval(taskIntervalMinutes));
      }
      catch (Exception ex)
      {
        _log.LogError($"FilterCleanupJob: Unable to schedule recurring job: DatabaseCleanup {ex.Message}");
        throw new Exception("FilterCleanupJob: Unable to schedule recurring job: DatabaseCleanup");
      }
    }

    /// <summary>
    /// cleanup transient filters over n minutes old
    /// </summary>
    /// <param name="filterDbConnectionString"></param>
    /// <param name="ageInMinutesToDelete"></param>
    public void DatabaseCleanupJob(string filterDbConnectionString, int ageInMinutesToDelete)
    {
      var cutoffActionUtcToDelete = DateTime.UtcNow.AddMinutes(-ageInMinutesToDelete).ToString("yyyy-MM-dd HH:mm:ss"); // mySql requires this format
      _log.LogTrace($"FilterCleanupJob.DatabaseCleanupJob: starting. cutoffActionUtcToDelete: {cutoffActionUtcToDelete}");
      Console.WriteLine($"FilterCleanupJob.DatabaseCleanupJob: starting. cutoffActionUtcToDelete: {cutoffActionUtcToDelete}");

      MySqlConnection dbConnection;
      try
      {
        dbConnection =new MySqlConnection(filterDbConnectionString);
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"FilterCleanupJob.DatabaseCleanupJob: open filter DB exeception {ex.Message}");
        Console.WriteLine($"FilterCleanupJob.DatabaseCleanupJob: open filter DB exeception {ex.Message}");
        throw new Exception("FilterCleanupJob.DatabaseCleanupJob: open database exception");
      }

      var empty = "\"";
      string deleteCommand = $"DELETE FROM Filter WHERE (Name = {empty}{empty} OR Name IS NULL) AND LastActionedUTC < {empty}{cutoffActionUtcToDelete}{empty}";
      int deletedCount = 0;
      try
      {
        deletedCount = dbConnection.Execute(deleteCommand, cutoffActionUtcToDelete);
        Console.WriteLine($"FilterCleanupJob.DatabaseCleanupJob: connectionString {dbConnection.ConnectionString} deleteCommand {deleteCommand} deletedCount {deletedCount}");
      }
      catch (Exception ex)
      {
        _log.LogError($"FilterCleanupJob.DatabaseCleanupJob: execute exeception {ex.Message}");
        Console.WriteLine($"FilterCleanupJob.DatabaseCleanupJob: execute exeception {ex.Message}");
        throw new Exception("FilterCleanupJob.DatabaseCleanupJob: delete from database exception");
      }
      finally
      {
        dbConnection.Close(); 
      }

      _log.LogTrace($"FilterCleanupJob.DatabaseCleanupJob: completed successfully. CutoffActionUtcDeleted: {cutoffActionUtcToDelete} deletedCount: {deletedCount}");
      //Console.WriteLine($"FilterCleanupJob.DatabaseCleanupJob: completed successfully. CutoffActionUtcDeleted: {cutoffActionUtcToDelete} deletedCount: {deletedCount}");
    }
  }
}
