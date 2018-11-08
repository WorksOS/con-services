using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileHarvester.Interfaces;

namespace VSS.Productivity3D.TagFileHarvesterTests.Mock
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
      return Task.Run(() =>
                      {
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                      }, token);
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

    public Task StartNewLimitedConcurrency2(Action action, CancellationToken token)
    {
      return Task.Run(() =>
      {
        Thread.Sleep(TimeSpan.FromSeconds(2));
      }, token);
    }

    public Tuple<int, int> Status()
    {
      return new Tuple<int, int>(1,1);
    }
  }
}
