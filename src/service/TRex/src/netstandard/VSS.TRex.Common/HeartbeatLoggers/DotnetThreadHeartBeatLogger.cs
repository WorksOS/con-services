using System.Threading;

namespace VSS.TRex.Common.HeartbeatLoggers
{
  public class DotnetThreadHeartBeatLogger
  {
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
