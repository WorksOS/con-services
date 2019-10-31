using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public class JobRunnerHealthCheck : HealthCheck
  {
    public JobRunnerHealthCheck() : base(nameof(JobRunnerHealthCheck))
    {
    }

    protected override ValueTask<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = new CancellationToken())
    {
      return new ValueTask<HealthCheckResult>(State ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
    }

    public static volatile bool State = true;
  }
}
  
