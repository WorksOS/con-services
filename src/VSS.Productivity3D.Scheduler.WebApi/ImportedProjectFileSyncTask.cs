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
  /// ImportedProjectFileSyncTask syncs the importedFiles table between 2 databases 
  ///   1) MySql Project.ImportedFiles
  ///   2) MSSql NH_OP.ImportedFiles 
  /// </summary>
  public class ImportedProjectFileSyncTask
  {
    private readonly ILogger _log;
    private readonly IConfigurationStore _configStore = null;
    private static int DefaultTaskIntervalDefaultMinutes { get; } = 5;

    /// <summary>
    /// Initializes the ImportedProjectFileSyncTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="loggerFactory"></param>
    public ImportedProjectFileSyncTask(IConfigurationStore configStore, ILoggerFactory loggerFactory)
    {
      Console.WriteLine($"ImportedProjectFileSyncTask configStore {configStore} loggerFactory: {loggerFactory}");
      _log = loggerFactory.CreateLogger<ImportedProjectFileSyncTask>();
      _configStore = configStore;  
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
      string importedProjectFile_MySQL_ConnectionString = ConnectionUtils.GetConnectionString(_configStore, _log, "_Project");
      _log.LogInformation($"ImportedProjectFileSyncTask: taskIntervalSeconds: {taskIntervalMinutes} importedProjectFile_MySQL_ConnectionString: {importedProjectFile_MySQL_ConnectionString}.");
      Console.WriteLine($"ImportedProjectFileSyncTask: taskIntervalSeconds: {taskIntervalMinutes} importedProjectFile_MySQL_ConnectionString: {importedProjectFile_MySQL_ConnectionString}.");

      try
      {
        RecurringJob.AddOrUpdate(ImportedProjectFileSyncTask,
          () => DatabaseSyncTask(importedProjectFile_MySQL_ConnectionString), Cron.MinuteInterval(taskIntervalMinutes));
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedProjectFileSyncTask: Unable to schedule recurring job: DatabaseCleanup {ex.Message}");
        throw new Exception("ImportedProjectFileSyncTask: Unable to schedule recurring job: DatabaseCleanup");
      }
    }

    /// <summary>
    /// bi-sync between 2 db tables
    /// </summary>
    /// <param name="importedProjectFile_MySQL_ConnectionString"></param>
    public void DatabaseSyncTask(string importedProjectFile_MySQL_ConnectionString)
    {
       _log.LogTrace($"ImportedProjectFileSyncTask.DatabaseSyncTask: starting. importedProjectFile_MySQL_ConnectionString {importedProjectFile_MySQL_ConnectionString}");
      Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: starting. importedProjectFile_MySQL_ConnectionString {importedProjectFile_MySQL_ConnectionString}");

      MySqlConnection dbConnection = new MySqlConnection(importedProjectFile_MySQL_ConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedProjectFileSyncTask.DatabaseSyncTask: open MySql DB exeception {ex.Message}");
        Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: open MySql DB exeception {ex.Message}");
        throw new Exception("ImportedProjectFileSyncTask.DatabaseSyncTask: open MySql database exception");
      }
      finally // todo temp
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: FailedToOpenDb dbConnection.Close");
      }

      //var empty = "\"";
      //string deleteCommand = $"SELECT FROM todo WHERE (Name = {empty}{empty} OR Name IS NULL) AND LastActionedUTC < {empty}{cutoffActionUtcToDelete}{empty}";
      //int deletedCount = 0;
      //try
      //{
      //  deletedCount = dbConnection.Execute(deleteCommand, cutoffActionUtcToDelete);
      //  Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: connectionString {dbConnection.ConnectionString} deleteCommand {deleteCommand} deletedCount {deletedCount}");
      //}
      //catch (Exception ex)
      //{
      //  _log.LogError($"ImportedProjectFileSyncTask.DatabaseSyncTask: execute exeception {ex.Message}");
      //  Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: execute exeception {ex.Message}");
      //  throw new Exception("ImportedProjectFileSyncTask.DatabaseSyncTask: delete from database exception");
      //}
      //finally
      //{
      //  dbConnection.Close();
      //Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: dbConnection.Close");
      //}

      //_log.LogTrace($"ImportedProjectFileSyncTask.DatabaseSyncTask: completed successfully. CutoffActionUtcDeleted: {cutoffActionUtcToDelete} deletedCount: {deletedCount}");
      ////Console.WriteLine($"ImportedProjectFileSyncTask.DatabaseSyncTask: completed successfully. CutoffActionUtcDeleted: {cutoffActionUtcToDelete} deletedCount: {deletedCount}");
    }
  }
}
