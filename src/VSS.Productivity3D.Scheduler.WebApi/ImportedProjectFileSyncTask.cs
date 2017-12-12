using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Controller;
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
    private readonly IConfigurationStore _configStore;
    private readonly ILoggerFactory _logger;
    private readonly ILogger _log;
    private readonly IRaptorProxy _raptorProxy;
    private readonly ITPaasProxy _tPaasProxy;
    private static int DefaultTaskIntervalDefaultMinutes { get; } = 4;

    /// <summary>
    /// Initializes the ImportedProjectFileSyncTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="tPaasProxy"></param>
    public ImportedProjectFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy, ITPaasProxy tPaasProxy)
    {
      _configStore = configStore;
      _logger = logger;
      _log = logger.CreateLogger<ImportedProjectFileSyncTask>();
      _raptorProxy = raptorProxy;
      _tPaasProxy = tPaasProxy;
    }

    /// <summary>
    /// Add a Task to the scheduler
    /// </summary>
    public void AddTask()
    {
      var startUtc = DateTime.UtcNow;

      // lowest interval is minutes 
      int taskIntervalMinutes;
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_TASK_INTERVAL_MINUTES"),
        out taskIntervalMinutes))
      {
        taskIntervalMinutes = DefaultTaskIntervalDefaultMinutes;
      }

      var ImportedProjectFileSyncTask = "ImportedProjectFileSyncTask";
      _log.LogInformation($"ImportedProjectFileSyncTask: taskIntervalMinutes: {taskIntervalMinutes}.");

      try
      {
        RecurringJob.AddOrUpdate(ImportedProjectFileSyncTask, () => ImportedFilesSyncTask(),
          Cron.MinuteInterval(taskIntervalMinutes));
      }
      catch (Exception ex)
      {
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message", string.Format($"Unable to schedule recurring job: exception {ex.Message}")}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Fatal", startUtc,
          (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
        throw;
      }
    }

    /// <summary>
    /// bi-sync between 2 databases, 1 table in each
    /// </summary>
    [AutomaticRetry(Attempts = 1, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [DisableConcurrentExecution(5)]
    public async Task ImportedFilesSyncTask()
    {
      var startUtc = DateTime.UtcNow;
      _log.LogDebug($"ImportedFilesSyncTask()  beginning. startUtc: {startUtc}");

      var sync = new ImportedFileSynchronizer(_configStore, _logger, _raptorProxy, _tPaasProxy);
      await sync.SyncTables().ConfigureAwait(false);

      var newRelicAttributes = new Dictionary<string, object> {
        { "message", string.Format($"Task completed.") }
      };
      NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
    }
  }
}
