using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;

namespace VSS.TRex.Common.HeartbeatLoggers
{
  public class DotnetThreadHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DotnetThreadHeartBeatLogger>();

    public void HeartBeat()
    {
      Log.LogInformation("Heartbeat: " + ToString());
    }

    public override string ToString()
    {
      ThreadPool.GetMaxThreads(out int maxWorkers, out int maxCompletionPortThreads);
      ThreadPool.GetAvailableThreads(out int availableWorkers, out int availableCompletionPortThreads);
      ThreadPool.GetMinThreads(out int minWorkers, out int minCompletionPortThreads);
      return $"Dotnet Threadpool Max[workers={maxWorkers} completion={maxCompletionPortThreads}] " +
        $"Available [workers={availableWorkers}, completion={availableCompletionPortThreads}] " +
        $"Min [wokers={minWorkers} completion{minCompletionPortThreads}]";
    }
  }
}
