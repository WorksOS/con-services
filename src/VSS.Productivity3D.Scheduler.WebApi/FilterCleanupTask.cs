using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Dapper;
using Hangfire;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
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
    private readonly IFilterRepository _filterRepository;
    private static int DefaultFilterAgeDefaultMinutes { get; } = 4;
    private static int DefaultTaskIntervalDefaultMinutes { get; } = 240; // 4 hours

    /// <summary>
    /// Initializes the FilterCleanupTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="filterRepository"></param>
    public FilterCleanupTask(IConfigurationStore configStore, ILoggerFactory loggerFactory, IFilterRepository filterRepository)
    {
      _log = loggerFactory.CreateLogger<FilterCleanupTask>();
      _configStore = configStore;
      

      if (filterRepository == null)
      {
        _log.LogError("filter repo is null!");
        throw new ArgumentNullException("FilterRepository is null");
      }
      _filterRepository = filterRepository;
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

      try
      {
        RecurringJob.AddOrUpdate(FilterCleanupTask,
          () => FilterTableCleanupTask(filterDbConnectionString, ageInMinutesToDelete), Cron.MinuteInterval(taskIntervalMinutes));
      }
      catch (Exception ex)
      {
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"Unable to schedule recurring job: exception {ex.Message}") }
        };
        NewRelicUtils.NotifyNewRelic("FilterTableCleanupTask", "Fatal", startUtc, _log, newRelicAttributes);
        throw;
      }
    }

    /// <summary>
    /// cleanup transient filters over n minutes old
    /// </summary>
    /// <param name="filterDbConnectionString"></param>
    /// <param name="ageInMinutesToDelete"></param>
    [AutomaticRetry(Attempts = 3, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [DisableConcurrentExecution(5)]
    public void FilterTableCleanupTask(string filterDbConnectionString, int ageInMinutesToDelete)
    {
      var startUtc = DateTime.UtcNow;
      var empty = "\"";
      _log.LogDebug($"FilterTableCleanupTask() beginning. startUtc: {startUtc}");

      Dictionary<string, object> newRelicAttributes;
      MySqlConnection dbConnection = new MySqlConnection(filterDbConnectionString);


      var cutoffActionUtcToDelete = startUtc.AddMinutes(-ageInMinutesToDelete).ToString("yyyy-MM-dd HH:mm:ss"); // mySql requires this format 

      _log.LogDebug($"FilterTableCleanupTask() getting filters to remove. startUtc: {startUtc}");
      //TODO: replace this with the filter repo once connection string config clash is sorted
      //var filtersToBeDeleted = _filterRepository.GetTransientFiltersToBeCleaned(ageInMinutesToDelete);

      //TODO: add exception handling

      var filterstoBeDeletedQuery = $@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_FilterTypeID = 1 
                AND f.LastActionedUTC < {empty}{cutoffActionUtcToDelete}{empty}";

      var filtersToBeDeleted = dbConnection.Query<Filter>(filterstoBeDeletedQuery).AsList();
      _log.LogInformation($"{Environment.NewLine}************** THE FOLLOWING FILTERS ARE GOING TO BE REMOVED ***************{Environment.NewLine}");
      foreach (var filter in filtersToBeDeleted)
      {
        _log.LogInformation(filter.ToString());
      }
      _log.LogInformation($"{Environment.NewLine}************** {filtersToBeDeleted.Count} FILTERS TO BE REMOVED ***************{Environment.NewLine}");

      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"open filter DB exeception {ex.Message}") }
        };
        NewRelicUtils.NotifyNewRelic("FilterTableCleanupTask", "Fatal", startUtc, _log, newRelicAttributes);
        throw;
      }
      finally
      {
        dbConnection.Close();
      }

 
      string deleteCommand = $"DELETE FROM Filter WHERE fk_FilterTypeID = 1 AND LastActionedUTC < {empty}{cutoffActionUtcToDelete}{empty}";
      int deletedCount = 0;
      try
      {
        deletedCount = dbConnection.Execute(deleteCommand, cutoffActionUtcToDelete);
        _log.LogTrace($"FilterCleanupTask.FilterTableCleanupTask: connectionString {dbConnection.ConnectionString} deletedCount {deletedCount}");
      }
      catch (Exception ex)
      {
        newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"execute delete on filter DB exeception {ex.Message}") }
        };
        NewRelicUtils.NotifyNewRelic("FilterTableCleanupTask", "Fatal", startUtc, _log, newRelicAttributes);
        throw;
      }
      finally
      {
        dbConnection.Close();
      }
      
      newRelicAttributes = new Dictionary<string, object> {
        { "message", string.Format($"Task completed.") }, { "ageInMinutesToDelete", ageInMinutesToDelete }, {"cutoffActionUtcToDelete", cutoffActionUtcToDelete }, {"deletedCount", deletedCount}
      };
      NewRelicUtils.NotifyNewRelic("FilterTableCleanupTask",  "Information", startUtc, _log, newRelicAttributes);
    }
  }
}
