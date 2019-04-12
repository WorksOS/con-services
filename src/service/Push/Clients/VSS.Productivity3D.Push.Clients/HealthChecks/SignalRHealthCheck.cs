using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;

namespace VSS.Productivity3D.Push.Clients
{
  public class SignalRHealthCheck : HealthCheck
  {
    public SignalRHealthCheck() : base(nameof(SignalRHealthCheck))
    {
    }

    protected override ValueTask<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = new CancellationToken())
    {
      return new ValueTask<HealthCheckResult>(State ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
    }

    public static volatile bool State;
  }
}