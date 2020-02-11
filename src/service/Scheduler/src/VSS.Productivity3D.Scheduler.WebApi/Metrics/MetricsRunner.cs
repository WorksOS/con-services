using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Scheduling;
using Hangfire;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI
{
  public class HangfireMetricsRunner
  {

    private readonly IMetrics _metrics;
    private const string retrySetName = "retries";

    public HangfireMetricsRunner(IMetrics metrics)
    {
      _metrics = metrics;
    }

    public void Run()
    {
      try
      {
        var stats = JobStorage.Current.GetMonitoringApi().GetStatistics();
        long retryJobs = JobStorage.Current.GetConnection().GetAllItemsFromSet(retrySetName).Count;

        _metrics.Measure.Gauge.SetValue(HangfireMetrics.EnqueuedJobs, () => stats.Enqueued);
        _metrics.Measure.Gauge.SetValue(HangfireMetrics.FailedJobs, () => stats.Failed);
        _metrics.Measure.Gauge.SetValue(HangfireMetrics.ProcessingJobs, () => stats.Processing);
        _metrics.Measure.Gauge.SetValue(HangfireMetrics.ScheduledJobs, () => stats.Scheduled);
        _metrics.Measure.Gauge.SetValue(HangfireMetrics.SucceededJobs, () => stats.Succeeded);
        _metrics.Measure.Gauge.SetValue(HangfireMetrics.RetryJobs, () => retryJobs);
      }
      catch { }

    }
  }

}
