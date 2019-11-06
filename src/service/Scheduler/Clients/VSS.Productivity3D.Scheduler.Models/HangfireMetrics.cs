using App.Metrics;
using App.Metrics.Gauge;

namespace VSS.Productivity3D.Scheduler.Models
{
  public static class HangfireMetrics
  {
    private static readonly string Context = "Hangfire";

    public static GaugeOptions FailedJobs = new GaugeOptions
    {
      Context = Context,
      Name = "Number of failed jobs",
      MeasurementUnit = Unit.Items
    };

    public static GaugeOptions EnqueuedJobs = new GaugeOptions
    {
      Context = Context,
      Name = "Number of enqueued jobs",
      MeasurementUnit = Unit.Items
    };

    public static GaugeOptions ScheduledJobs = new GaugeOptions
    {
      Context = Context,
      Name = "Number of scheduled jobs",
      MeasurementUnit = Unit.Items
    };

    public static GaugeOptions ProcessingJobs = new GaugeOptions
    {
      Context = Context,
      Name = "Number of processing jobs",
      MeasurementUnit = Unit.Items
    };

    public static GaugeOptions SucceededJobs = new GaugeOptions
    {
      Context = Context,
      Name = "Number of succeeded jobs",
      MeasurementUnit = Unit.Items
    };

    public static GaugeOptions RetryJobs = new GaugeOptions
    {
      Context = Context,
      Name = "Retrying jobs",
      MeasurementUnit = Unit.Events
    };
  }
}
