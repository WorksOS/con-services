using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;

namespace VSS.MasterData.Repositories
{
  public class MySqlHealthCheck : HealthCheck
  {
    public MySqlHealthCheck() : base(nameof(MySqlHealthCheck))
    {
    }

    protected override ValueTask<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = new CancellationToken())
    {
      lock (lockObj)
      {
        if (MySqlHealthCheck.statuses.Any(v => !v.Value))
          return new ValueTask<HealthCheckResult>(HealthCheckResult.Unhealthy(statuses.Where(v => !v.Value)
            .Select(v => v.Key.FullName).Aggregate((s, s1) => $"{s};{s1}")));
        return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy());
      }
    }

    public static void SetStatus(bool status, Type objectType)
    {
      lock (lockObj)
      {

        statuses[objectType] = status;
      }
    }

    private static object lockObj = new object();
    private static Dictionary<Type,bool> statuses = new Dictionary<Type, bool>();
  }
}