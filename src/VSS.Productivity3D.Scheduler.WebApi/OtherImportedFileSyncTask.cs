using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.Productivity3D.Scheduler.WebApi;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Scheduler.WebAPI
{


  public class SkipWhenPreviousJobIsRunningAttribute : JobFilterAttribute, IClientFilter, IApplyStateFilter
  {
    public void OnCreating(CreatingContext context)
    {
      var connection = context.Connection as JobStorageConnection;

      // We can't handle old storages
      if (connection == null) return;

      // We should run this filter only for background jobs based on 
      // recurring ones
      if (!context.Parameters.ContainsKey("RecurringJobId")) return;

      var recurringJobId = context.Parameters["RecurringJobId"] as string;

      // RecurringJobId is malformed. This should not happen, but anyway.
      if (String.IsNullOrWhiteSpace(recurringJobId)) return;

      var running = connection.GetValueFromHash($"recurring-job:{recurringJobId}", "Running");
      if ("yes".Equals(running, StringComparison.OrdinalIgnoreCase))
      {
        context.Canceled = true;
      }
    }

    public void OnCreated(CreatedContext filterContext)
    {
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
      if (context.NewState is EnqueuedState)
      {
        var recurringJobId = JobHelper.FromJson<string>(context.Connection.GetJobParameter(context.BackgroundJob.Id, "RecurringJobId"));
        if (String.IsNullOrWhiteSpace(recurringJobId)) return;

        transaction.SetRangeInHash(
            $"recurring-job:{recurringJobId}",
            new[] { new KeyValuePair<string, string>("Running", "yes") });
      }
      else if (context.NewState.IsFinal)
      {
        var recurringJobId = JobHelper.FromJson<string>(context.Connection.GetJobParameter(context.BackgroundJob.Id, "RecurringJobId"));
        if (String.IsNullOrWhiteSpace(recurringJobId)) return;

        transaction.SetRangeInHash(
            $"recurring-job:{recurringJobId}",
            new[] { new KeyValuePair<string, string>("Running", "no") });
      }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
  }


  /// <summary>
  /// Task for processing and sync'ing imported files other than surveyed surfaces.
  /// </summary>
  public class OtherImportedFileSyncTask : ImportedProjectFileSyncTask
  {
    public OtherImportedFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy,
      ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo) :
      base(configStore, logger, raptorProxy, tPaasProxy, impFileProxy, fileRepo)
    {
    }

    /// <summary>
    /// This is just so we can have the Hangfire attributes on different methods for different tasks
    /// </summary>
    [AutomaticRetry(Attempts = 1, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    //[DisableConcurrentExecution(0)]
    [SkipWhenPreviousJobIsRunning]
    public void OtherImportedFilesSyncTask()
    {
      ImportedFilesSyncTask(false);
    }

    /// <summary>
    /// Add a Task to the scheduler
    /// </summary>
    public void AddTask()
    {
      var startUtc = DateTime.UtcNow;

      // lowest interval is minutes 
      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_NonSS_TASK_INTERVAL_MINUTES"),
        out int taskIntervalMinutes))
      {
        taskIntervalMinutes = DefaultTaskIntervalDefaultMinutes;
      }

      var importedProjectFileSyncTask = "ImportedProjectFileSyncNonSurveyedSurfaceTask";
      _log.LogInformation($"ImportedProjectFileSyncTask: (processNonSurveyedSurfaceType) taskIntervalMinutes: {taskIntervalMinutes}.");

      try
      {
        RecurringJob.AddOrUpdate(importedProjectFileSyncTask, () => OtherImportedFilesSyncTask(),
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
