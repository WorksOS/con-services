using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Controller;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// ImportedProjectFileSyncTask syncs the importedFiles table between 2 databases 
  ///   1) MySql Project.ImportedFiles
  ///   2) MSSql NH_OP.ImportedFiles 
  /// </summary>
  public abstract class ImportedProjectFileSyncTask
  {
    private readonly IConfigurationStore _configStore;
    private readonly ILoggerFactory _logger;
    private readonly ILogger _log;
    private readonly IRaptorProxy _raptorProxy;
    private readonly ITPaasProxy _tPaasProxy;
    private readonly IImportedFileProxy _impFileProxy;
    private readonly IFileRepository _fileRepo;
    private static int DefaultTaskIntervalDefaultMinutes { get; } = 4;

    /// <summary>
    /// Gets or sets whether the file sync task is for surveyed surface.
    /// </summary>
    protected bool ProcessSurveyedSurfaceType { get; set; }

    /// <summary>
    /// Initializes the ImportedProjectFileSyncTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="tPaasProxy"></param>
    /// <param name="impFileProxy"></param>
    /// <param name="fileRepo"></param>
    public ImportedProjectFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy,
      ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo)
    {
      _configStore = configStore;
      _logger = logger;
      _log = logger.CreateLogger<ImportedProjectFileSyncTask>();
      _raptorProxy = raptorProxy;
      _tPaasProxy = tPaasProxy;
      _impFileProxy = impFileProxy;
      _fileRepo = fileRepo;
    }

    /// <summary>
    /// Add a Task to the scheduler
    /// </summary>
    public void AddTask()
    {
      _log.LogDebug($"AddTask: ProcessSurveyedSurfaceType={ProcessSurveyedSurfaceType}");
      var startUtc = DateTime.UtcNow;

      // lowest interval is minutes 
      if (!int.TryParse(_configStore.GetValueString((ProcessSurveyedSurfaceType ? "SCHEDULER_IMPORTEDPROJECTFILES_SYNC_SS_TASK_INTERVAL_MINUTES" : "SCHEDULER_IMPORTEDPROJECTFILES_SYNC_NonSS_TASK_INTERVAL_MINUTES")),
        out int taskIntervalMinutes))
      {
        taskIntervalMinutes = DefaultTaskIntervalDefaultMinutes;
      }

      var importedProjectFileSyncTask = (ProcessSurveyedSurfaceType ? "ImportedProjectFileSyncSurveyedSurfaceTask" : "ImportedProjectFileSyncNonSurveyedSurfaceTask");
      _log.LogInformation($"ImportedProjectFileSyncTask: ({(ProcessSurveyedSurfaceType ? "processSurveyedSurfaceType" : "processNonSurveyedSurfaceType")}) taskIntervalMinutes: {taskIntervalMinutes}.");

      try
      {
        RecurringJob.AddOrUpdate(importedProjectFileSyncTask, () => ImportedFilesSyncTask(),
          Cron.MinuteInterval(taskIntervalMinutes));
      }
      catch (Exception ex)
      {
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message", string.Format($"Unable to schedule recurring job: exception {ex.Message}")}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Fatal", startUtc, _log, newRelicAttributes);
        throw;
      }
    }

    /// <summary>
    /// bi-sync between 2 databases, 1 table in each
    /// </summary>
    [AutomaticRetry(Attempts = 1, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [DisableConcurrentExecution(5)]
    public void ImportedFilesSyncTask()
    {
      _log.LogDebug($"ImportedFilesSyncTask: ProcessSurveyedSurfaceType={ProcessSurveyedSurfaceType}");

      var startUtc = DateTime.UtcNow;
      _log.LogDebug($"ImportedFilesSyncTask()  beginning. startUtc: {startUtc}");

      var sync = new ImportedFileSynchronizer(_configStore, _logger, _raptorProxy, _tPaasProxy, _impFileProxy, _fileRepo, ProcessSurveyedSurfaceType);
      sync.SyncTables().Wait();

      var newRelicAttributes = new Dictionary<string, object> {
        { "message", "Task completed." }
      };
      NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log, newRelicAttributes);
    }
  }
}
