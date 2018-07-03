using System;
using System.Threading;
using System.Threading.Tasks;
using TagFileHarvester.Interfaces;

namespace TagFileHarvesterTests.Mock
{
  public class MockTaskScheduler : IHarvesterTasks
  {
    public Task<T> StartNewLimitedConcurrency<T>(Func<T> action, CancellationToken token)
    {
      return Task.Run<T>(() =>
      {
        Thread.Sleep(TimeSpan.FromSeconds(2));
        return null;
      }, token);
    }

    public Task StartNewLimitedConcurrency(Action action, CancellationToken token)
    {
      return Task.Run(() => { Thread.Sleep(TimeSpan.FromSeconds(2)); }, token);
    }

    public Task<T> StartNewLimitedConcurrency2<T>(Func<T> action, CancellationToken token)
    {
      return Task.Run<T>(() =>
      {
        Thread.Sleep(TimeSpan.FromSeconds(2));
        return null;
      }, token);
    }

    public Task StartNewLimitedConcurrency2(Action action, CancellationToken token, bool delay)
    {
      throw new NotImplementedException();
    }

    public Tuple<int, int> Status()
    {
      return new Tuple<int, int>(1, 1);
    }

    public Task StartNewLimitedConcurrency2(Action action, CancellationToken token)
    {
      return Task.Run(() => { Thread.Sleep(TimeSpan.FromSeconds(2)); }, token);
    }
  }
}