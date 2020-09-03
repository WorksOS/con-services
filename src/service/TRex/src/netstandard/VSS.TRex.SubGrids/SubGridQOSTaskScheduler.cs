using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Implements a very simple task scheduler that takes a collection of collections of sub grids to be processed and
  /// provides a bound on the number of tasks used to process them
  /// </summary>
  public static class SubGridQOSTaskScheduler
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger("SubGridQOSTaskScheduler");

    /// <summary>
    /// The count of tasks the QOS scheduler has scheduled or execution on the .Net task thread pool.
    /// </summary>
    private static int _currentExecutingTaskCount = 0;

    /// <summary>
    /// The count of scheduler sessions representing sets of sub grid requests required for various TRex requests.
    /// </summary>
    private static int _currentSchedulerSessionsCount = 0;

    public const int DEFAULT_THREAD_POOL_FRACTION_DIVISOR = 8;

    /// <summary>
    /// The number of seconds the QOS scheduler will wait for a group of tasks responsible for processing sub grids
    /// </summary>
    public const int TASK_GROUP_TIMEOUT_SECONDS = 10;

    private static readonly object _capacityLock = new object();

    /// <summary>
    /// The flag to indicate the service has been terminated (eg: via a SIG_TERM message), and so should abort active operations
    /// </summary>
    public static bool Terminated = false;

    /// <summary>
    /// Provides an estimation of the default maximum number of tasks that may be used to provide service to other
    /// requests active at the same time when using the default system thread pool. This number is based on the
    /// minimum number of threads specified for the thread pool.
    /// </summary>
    public static int DefaultMaxTasks()
    {
      ThreadPool.GetMinThreads(out var minWorkerThreads, out _);

      return minWorkerThreads / DEFAULT_THREAD_POOL_FRACTION_DIVISOR;
    }

    private static bool WaitForGroupToComplete(List<Task> tasks)
    {
      if (tasks.Count == 0)
        return true;

      var taskCount = tasks.Count;

      try
      {
        try
        {
          if (!Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(TASK_GROUP_TIMEOUT_SECONDS)))
          {
            _log.LogError($"Task group failed to complete within {TASK_GROUP_TIMEOUT_SECONDS} seconds");
          }

          tasks.Clear();

          return true;
        }
        catch (Exception e)
        {
          _log.LogError(e, "Exception waiting for group of sub grid tasks to complete");
          return false;
        }
      }
      finally
      {
        Interlocked.Add(ref _currentExecutingTaskCount, -taskCount);
      }
    }

    /// <summary>
    /// Determine if there is sufficient capacity to create another task
    /// </summary>
    private static bool WaitForCapacity()
    {
      ThreadPool.GetMinThreads(out var capacityCap, out _);

      while (!Terminated && _currentExecutingTaskCount >= capacityCap)
      {
        // Sleep for 10 milliseconds and check again
        Thread.Sleep(10);
      }

      return !Terminated;
    }

    /// <summary>
    /// Take a collection of sub grids, a functor to process each group, and a cap of available tasks
    /// </summary>
    /// <param name="subGridCollections">The group of sub grid address collections to be processed</param>
    /// <param name="processor">The lambda responsible for processing them</param>
    /// <param name="maxTasks">The maximum number of tasks that may be used</param>
    public static bool Schedule(List<SubGridCellAddress[]> subGridCollections,
      Action<SubGridCellAddress[]> processor,
      int maxTasks)
    {
      var collectionCount = subGridCollections?.Count ?? 0;
      var taskIndex = 0;

      var preActiveContexts = Interlocked.Increment(ref _currentSchedulerSessionsCount);

      _log.LogInformation($"Sub grid QOS scheduler starting {collectionCount} collections across {maxTasks} tasks. {preActiveContexts} sessions are active");

      try
      {
        if (collectionCount == 0 || subGridCollections == null)
          return true;

        var tasks = new List<Task>(maxTasks);

        foreach (var subGridCollection in subGridCollections)
        {
          if (Terminated)
          {
            return false;
          }

          lock (_capacityLock)
          {
            if (!WaitForCapacity())
              return false;

            Interlocked.Increment(ref _currentExecutingTaskCount);
          }

          tasks.Add(Task.Run(() =>
          {
            try
            {
              // ReSharper disable once AccessToModifiedClosure
              _log.LogDebug($"Processor for task index {taskIndex} starting");

              processor(subGridCollection);

              // ReSharper disable once AccessToModifiedClosure
              _log.LogDebug($"Processor for task index {taskIndex} completed");
            }
            catch (Exception e)
            {
              _log.LogError(e, "Exception processing group of sub grids");
              throw;
            }
          }));

          if (tasks.Count < maxTasks)
            continue;

          if (!WaitForGroupToComplete(tasks))
            return false;

          taskIndex++;
        }

        return WaitForGroupToComplete(tasks);
      }
      finally
      {
        var postActiveContexts = Interlocked.Decrement(ref _currentSchedulerSessionsCount);
        _log.LogInformation($"Sub grid QOS scheduler completed {collectionCount} collections across {maxTasks} tasks. {postActiveContexts} sessions are active.");
      }
    }
  }
}
