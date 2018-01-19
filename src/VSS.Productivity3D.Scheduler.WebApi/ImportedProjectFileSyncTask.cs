using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
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
  public class ImportedProjectFileSyncTask
  {
    private readonly IConfigurationStore _configStore;
    private readonly ILoggerFactory _logger;
    private readonly ILogger _log;
    private readonly IRaptorProxy _raptorProxy;
    private readonly ITPaasProxy _tPaasProxy;
    private readonly IImportedFileProxy _impFileProxy;
    private readonly IFileRepository _fileRepo;
    private static int DefaultTaskIntervalDefaultMinutes { get; } = 4;
    private bool _processSurveyedSurfaceType = false;

    /// <summary>
    /// Initializes the ImportedProjectFileSyncTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="tPaasProxy"></param>
    /// <param name="impFileProxy"></param>
    /// <param name="fileRepo"></param>
    /// <param name="processSurveyedSurfaceType"></param>
    public ImportedProjectFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy, 
      ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo, bool processSurveyedSurfaceType)
    {
      _configStore = configStore;
      _logger = logger;
      _log = logger.CreateLogger<ImportedProjectFileSyncTask>();
      _raptorProxy = raptorProxy;
      _tPaasProxy = tPaasProxy;
      _impFileProxy = impFileProxy;
      _fileRepo = fileRepo;
      _processSurveyedSurfaceType = processSurveyedSurfaceType;
    }

    /// <summary>
    /// Add a Task to the scheduler
    /// </summary>
    public void AddTask()
    {
      var startUtc = DateTime.UtcNow;

      // lowest interval is minutes 
      int taskIntervalMinutes;
      if (!int.TryParse(_configStore.GetValueString((_processSurveyedSurfaceType ? "SCHEDULER_IMPORTEDPROJECTFILES_SYNC_SS_TASK_INTERVAL_MINUTES" : "SCHEDULER_IMPORTEDPROJECTFILES_SYNC_NonSS_TASK_INTERVAL_MINUTES")),
        out taskIntervalMinutes))
      {
        taskIntervalMinutes = DefaultTaskIntervalDefaultMinutes;
      }

      var ImportedProjectFileSyncTask = "ImportedProjectFileSyncTask";
      _log.LogInformation($"ImportedProjectFileSyncTask: ({(_processSurveyedSurfaceType ? "processSurveyedSurfaceType" : "processNonSurveyedSurfaceType")}) taskIntervalMinutes: {taskIntervalMinutes}.");

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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Fatal", startUtc, _log, newRelicAttributes);
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

      var sync = new ImportedFileSynchronizer(_configStore, _logger, _raptorProxy, _tPaasProxy, _impFileProxy, _fileRepo, _processSurveyedSurfaceType);
      await sync.SyncTables().ConfigureAwait(false);

      var newRelicAttributes = new Dictionary<string, object> {
        { "message", string.Format($"Task completed.") }
      };
      NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log, newRelicAttributes);
    }
  }
}
