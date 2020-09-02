using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
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

    public const int DEFAULT_THREAD_POOL_FRACTION_DIVISOR = 8;

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

      try
      {
        Task.WhenAll(tasks).WaitAndUnwrapException();
        tasks.Clear();

        return true;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception waiting for group of sub grid tasks to complete");
        return false;
      }
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

      _log.LogInformation($"Sub grid QOS scheduler running {collectionCount} collections across {maxTasks} tasks");

      try
      {
        if (collectionCount == 0 || subGridCollections == null)
          return true;

        var tasks = new List<Task>(maxTasks);

        foreach (var subGridCollection in subGridCollections)
        {
          tasks.Add(Task.Run(() =>
          {
            try
            {
              processor(subGridCollection);
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
        }

        return WaitForGroupToComplete(tasks);
      }
      finally
      {
        _log.LogInformation($"Sub grid QOS scheduler completed {collectionCount} collections across {maxTasks} tasks");
      }
    }
  }
}
