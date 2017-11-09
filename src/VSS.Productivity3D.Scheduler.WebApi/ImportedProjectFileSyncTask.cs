using System;
using Hangfire;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Models;


namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// ImportedProjectFileSyncTask syncs the importedFiles table between 2 databases 
  ///   1) MySql Project.ImportedFiles
  ///   2) MSSql NH_OP.ImportedFiles 
  /// </summary>
  public class ImportedProjectFileSyncTask
  {
    private readonly IConfigurationStore _configStore;
    private readonly ILoggerFactory _logger;
    private readonly ILogger _log;
    private static int DefaultTaskIntervalDefaultMinutes { get; } = 5;

    /// <summary>
    /// Initializes the ImportedProjectFileSyncTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    public ImportedProjectFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger)
    {
      Console.WriteLine($"ImportedProjectFileSyncTask configStore {configStore} logger: {logger}");
      _configStore = configStore;
      _logger = logger;
      _log = logger.CreateLogger<ImportedProjectFileSyncTask>();
    }

    /// <summary>
    /// Add a Task to the scheduler
    /// </summary>
    public void AddTask()
    {
      _log.LogDebug($"ImportedProjectFileSyncTask.AddTask. configStore: {_configStore}");
     
      // lowest interval is minutes 
      int taskIntervalMinutes;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_TASK_INTERVAL_MINUTES"), out taskIntervalMinutes))
      {
        taskIntervalMinutes = DefaultTaskIntervalDefaultMinutes;
      }

      var ImportedProjectFileSyncTask = "ImportedProjectFileSyncTask";
      _log.LogInformation($"ImportedProjectFileSyncTask: taskIntervalSeconds: {taskIntervalMinutes}.");
      Console.WriteLine($"ImportedProjectFileSyncTask: taskIntervalSeconds: {taskIntervalMinutes}.");

      try
      {
        RecurringJob.AddOrUpdate(ImportedProjectFileSyncTask,() => DatabaseSyncTask(), Cron.MinuteInterval(taskIntervalMinutes));
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedProjectFileSyncTask: Unable to schedule recurring job: DatabaseCleanup {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// bi-sync between 2 databases, 1 table in each
    /// </summary>
    public void DatabaseSyncTask()
    {
      _log.LogTrace($"ImportedProjectFileSyncTask.DatabaseSyncTask: starting. nowUtc {DateTime.UtcNow}");
      Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: starting. nowUtc {DateTime.UtcNow}");

      var sync = new ImportedFileSynchronizer(_configStore, _logger);
      sync.SyncTables();

      _log.LogTrace($"ImportedProjectFileSyncTask.DatabaseSyncTask: completed successfully.");
      Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: completed successfully.");
    }
  }
}
