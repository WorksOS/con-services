using System;
using System.Threading;
using System.Threading.Tasks;

namespace TagFileHarvester.Interfaces
{
  public interface IHarvesterTasks
  {
    Task<T> StartNewLimitedConcurrency<T>(Func<T> action, CancellationToken token);
    Task StartNewLimitedConcurrency(Action action, CancellationToken token);
    Task<T> StartNewLimitedConcurrency2<T>(Func<T> action, CancellationToken token);
    Task StartNewLimitedConcurrency2(Action action, CancellationToken token, bool delay);
    Tuple<int, int> Status();
  }
}