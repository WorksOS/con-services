using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common.Extensions;
using VSS.TRex.DI;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Implements a very simple task scheduler that takes a collection of collections of sub grids to be processed and
  /// provides a bound on the number of tasks used to process them
  /// </summary>
  public class SubGridQOSTaskScheduler : ISubGridQOSTaskScheduler
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger("SubGridQOSTaskScheduler");

    /// <summary>
    /// The divisor used to indicate the largest fraction of task thread pool resources that may be applied to a single scheduler session
    /// This is also used as a multiplier against the default number of threads for that thread pool at system start up
    /// </summary>
    public int DefaultThreadPoolFractionDivisor { get; } = DIContext.Obtain<IConfigurationStore>().GetValueInt("TREX_QOS_SCHEDULER_DEFAULT_THREAD_POOL_FRACTION_DIVISOR", 8);

    /// <summary>
    /// The number of seconds the QOS scheduler will wait for a group of tasks responsible for processing sub grids
    /// </summary>
    private readonly int _taskGroupTimeoutSeconds = DIContext.Obtain<IConfigurationStore>().GetValueInt("TREX_QOS_SCHEDULER_TASK_GROUP_TIMEOUT_SECONDS", 10);

    private readonly int _maxConcurrentSchedulerSessions = DIContext.Obtain<IConfigurationStore>().GetValueInt("TREX_QOS_SCHEDULER_MAX_CONCURRENT_SCHEDULER_SESSIONS", -1);
    public int MaxConcurrentSchedulerSessions => _maxConcurrentSchedulerSessions;

    private readonly int _maxConcurrentSchedulerTasks;
    public int MaxConcurrentSchedulerTasks => _maxConcurrentSchedulerTasks;

    /// <summary>
    /// The semaphore used to restrict the number of concurrent scheduler sessions
    /// </summary>
    private SemaphoreSlim _sessionGatewaySemaphore;

    /// <summary>
    /// The semaphore used to restrict the number of concurrent tasks scheduled by separate sessions
    /// </summary>
    private SemaphoreSlim _taskGatewaySemaphore;

    /// <summary>
    /// The number of scheduler sessions that are concurrently executing
    /// </summary>
    public int CurrentExecutingSessionCount => _maxConcurrentSchedulerSessions - _sessionGatewaySemaphore.CurrentCount;

    /// <summary>
    /// The number of tasks being utilised by the concurrently running scheduler sessions
    /// </summary>
    public int CurrentExecutingTaskCount => _maxConcurrentSchedulerTasks - _taskGatewaySemaphore.CurrentCount;

    private int _totalSchedulerSessions = 0;

    /// <summary>
    /// The total number of scheduler sessions active, either pending execution, or being executed
    /// </summary>
    public int TotalSchedulerSessions => _totalSchedulerSessions;

    /// <summary>
    /// The flag to indicate the service has been terminated (eg: via a SIG_TERM message), and so should abort active operations
    /// </summary>
    public bool Terminated = false;

    public SubGridQOSTaskScheduler()
    {
      ThreadPool.GetMinThreads(out _maxConcurrentSchedulerTasks, out _);

      // If the maximum number of concurrent sessions is not configured in the options,
      // set the maximum number of concurrent sessions to half the number of concurrent tasks
      if (_maxConcurrentSchedulerSessions < 0)
      {
        _maxConcurrentSchedulerSessions = _maxConcurrentSchedulerTasks == 1 ? 1 : _maxConcurrentSchedulerTasks / 2;
      }

      CreateGatewaySemaphores();
    }

    public SubGridQOSTaskScheduler(int maxSchedulerSessions, int maxSchedulerTasks)
    {
      _maxConcurrentSchedulerSessions = maxSchedulerSessions;
      _maxConcurrentSchedulerTasks = maxSchedulerTasks;

      CreateGatewaySemaphores();
    }

    private void CreateGatewaySemaphores()
    {
      _sessionGatewaySemaphore = new SemaphoreSlim(MaxConcurrentSchedulerSessions, MaxConcurrentSchedulerSessions);
      _taskGatewaySemaphore = new SemaphoreSlim(MaxConcurrentSchedulerTasks, MaxConcurrentSchedulerTasks);
    }

    /// <summary>
    /// Provides an estimation of the default maximum number of tasks that may be used to provide service to other
    /// requests active at the same time when using the default system thread pool. This number is based on the
    /// minimum number of threads specified for the thread pool.
    /// </summary>
    public int DefaultMaxTasks()
    {
      ThreadPool.GetMinThreads(out var minWorkerThreads, out _);

      return minWorkerThreads / DefaultThreadPoolFractionDivisor;
    }

    private bool WaitForGroupToComplete(List<Task> tasks)
    {
      if (tasks == null)
        return true;

      var taskCount = tasks.Count;

      if (taskCount == 0)
        return true;

      try
      {
        if (!Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(_taskGroupTimeoutSeconds)))
        {
          _log.LogError($"Task group failed to complete within {_taskGroupTimeoutSeconds} seconds");
        }

        return true;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception waiting for group of sub grid tasks to complete");
        return false;
      }
      finally
      {
        tasks.Clear();
        _taskGatewaySemaphore.Release(taskCount);
      }
    }

    /// <summary>
    /// Take a collection of sub grids, a functor to process each group, and a cap of available tasks
    /// </summary>
    /// <param name="subGridCollections">The group of sub grid address collections to be processed</param>
    /// <param name="processor">The lambda responsible for processing them</param>
    /// <param name="maxTasks">The maximum number of tasks that may be used</param>
    public bool Schedule(List<SubGridCellAddress[]> subGridCollections,
      Action<SubGridCellAddress[]> processor,
      int maxTasks)
    {
      if (subGridCollections == null)
        return true;

      var collectionCount = subGridCollections.Count;

      if (collectionCount == 0)
        return true;

      var taskIndex = 0;
      var tasks = new List<Task>(maxTasks);

      _log.LogInformation($"Sub grid QOS scheduler starting {collectionCount} collections across {maxTasks} tasks. {CurrentExecutingSessionCount} sessions (of {_totalSchedulerSessions}) are active using {CurrentExecutingTaskCount} tasks");

      Interlocked.Increment(ref _totalSchedulerSessions);

      _sessionGatewaySemaphore.Wait();
      try
      {
        subGridCollections.ForEach((subGridCollection, subGridCollectionIndex) =>
        {
          if (Terminated)
          {
            return;
          }

          _taskGatewaySemaphore.Wait();

          tasks.Add(Task.Run(() =>
          {
            var localTaskindex = taskIndex;
            try
            {
              // ReSharper disable once AccessToModifiedClosure
              _log.LogDebug($"Processor for task index {localTaskindex} starting");

              processor(subGridCollection);

              // ReSharper disable once AccessToModifiedClosure
              _log.LogDebug($"Processor for task index {localTaskindex} completed");
            }
            catch (Exception e)
            {
              _log.LogError(e, "Exception processing group of sub grids");
              throw;
            }
          }));

          if (tasks.Count >= maxTasks || (subGridCollectionIndex + 1) >= collectionCount)
          {
            if (!WaitForGroupToComplete(tasks))
              return;
          }

          taskIndex++;
        });

        return !Terminated;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception processing QOS scheduler tasks");
        throw;
      }
      finally
      {
        // Ensure any pending tasks are removed from the task gateway semaphore
        if (tasks.Count > 0)
        {
          _taskGatewaySemaphore.Release(tasks.Count);
        }

        // Release the gateway semaphore to allow another scheduler to enter
        _sessionGatewaySemaphore.Release();

        Interlocked.Decrement(ref _totalSchedulerSessions);

        _log.LogInformation($"Sub grid QOS scheduler completed {collectionCount} collections across {maxTasks} tasks. {CurrentExecutingSessionCount} sessions (of {_totalSchedulerSessions}) are active using {CurrentExecutingTaskCount} tasks");
      }
    }
  }
}
