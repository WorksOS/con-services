using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Scheduling;

namespace VSS.Productivity3D.Scheduler.WebAPI.Metrics
{
  public class HangfireMetricScheduler : IHangfireMetricScheduler
  {

    private readonly IMetrics metrics;

    public HangfireMetricScheduler(IMetrics metrics)
    {
      this.metrics = metrics;
    }

    public void Start()
    {
      var processSample = new HangfireMetricsRunner(metrics);
      var samplesScheduler = new AppMetricsTaskScheduler(
               TimeSpan.FromMilliseconds(1000),
               () =>
               {
                 processSample.Run();
                 return Task.CompletedTask;
               });

      samplesScheduler.Start();
    }
  }
}
