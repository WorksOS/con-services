using System;
using System.Threading;
using System.Threading.Tasks;
using TagFileHarvester.Interfaces;
using TagFileHarvester.TaskQueues;

namespace TagFileHarvester.Implementation
{
  public class LimitedConcurrencyHarvesterTasks : IHarvesterTasks
  {
    private static readonly LimitedConcurrencyLevelTaskScheduler orgsScheduler =
      new LimitedConcurrencyLevelTaskScheduler(OrgsHandler.MaxThreadsToProcessTagFiles);

    private static readonly TaskFactory factory = new TaskFactory(orgsScheduler);

    private static readonly LimitedConcurrencyLevelTaskScheduler orgsScheduler2 =
      new LimitedConcurrencyLevelTaskScheduler(OrgsHandler.MaxThreadsToProcessTagFiles);

    private static readonly TaskFactory factory2 = new TaskFactory(orgsScheduler2);

    public Task<T> StartNewLimitedConcurrency<T>(Func<T> action, CancellationToken token)
    {
      return factory.StartNew(action, token);
    }

    public Task StartNewLimitedConcurrency(Action action, CancellationToken token)
    {
      return factory.StartNew(action, token);
    }

    public Task<T> StartNewLimitedConcurrency2<T>(Func<T> action, CancellationToken token)
    {
      return factory2.StartNew(action, token);
    }

    public Task StartNewLimitedConcurrency2(Action action, CancellationToken token, bool delay = false)
    {
      /* if (!delay)*/
      return factory2.StartNew(action, token);
      /*return Task.Factory.StartNew(() =>
      {
        Task.Delay(OrgsHandler.OrgProcessingDelay, token).ContinueWith((t) =>
          action.Invoke(), token).Wait(token);
      }, token);*/
    }

    public Tuple<int, int> Status()
    {
      return new Tuple<int, int>(orgsScheduler.ScheduledTasks, orgsScheduler2.ScheduledTasks);
    }
  }
}