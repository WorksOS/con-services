using System;
using System.Collections.Generic;
using Hangfire;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.Productivity3D.Scheduler.WebApi;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Scheduler.WebAPI
{
  /// <summary>
  /// Task for processing and sync'ing imported surveyed surface files.
  /// </summary>
  public class SurveyedSurfaceFileSyncTask : ImportedProjectFileSyncTask
  {
    public SurveyedSurfaceFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy,
      ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo) :
      base (configStore, logger, raptorProxy, tPaasProxy, impFileProxy, fileRepo)
    {
    }

    /// <summary>
    /// This is just so we can have the Hangfire attributes on different methods for different tasks
    /// </summary>
    [AutomaticRetry(Attempts = 1, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [DisableConcurrentExecution(5)]
    public void SurveyedSurfaceFilesSyncTask()
    {
      ImportedFilesSyncTask(true);
    }

    /// <summary>
    /// Add a Task to the scheduler
    /// </summary>
    public void AddTask()
    {
      var startUtc = DateTime.UtcNow;

      // lowest interval is minutes 
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_SS_TASK_INTERVAL_MINUTES"),
        out int taskIntervalMinutes))
      {
        taskIntervalMinutes = DefaultTaskIntervalDefaultMinutes;
      }

      var importedProjectFileSyncTask = "ImportedProjectFileSyncSurveyedSurfaceTask";
      _log.LogInformation($"ImportedProjectFileSyncTask: (processSurveyedSurfaceType) taskIntervalMinutes: {taskIntervalMinutes}.");

      try
      {
        RecurringJob.AddOrUpdate(importedProjectFileSyncTask, () => SurveyedSurfaceFilesSyncTask(),
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
  }
}
