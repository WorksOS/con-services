using System;
using System.Collections.Generic;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.SubGrids.Interfaces
{
  public interface ISubGridQOSTaskScheduler
  {
    bool Schedule(List<SubGridCellAddress[]> subGridCollections,
      Action<SubGridCellAddress[]> processor,
      int maxTasks);

    int DefaultMaxTasks();

    int DefaultThreadPoolFractionDivisor { get; }

    int MaxConcurrentSchedulerSessions { get; }
    int MaxConcurrentSchedulerTasks { get; }

    int CurrentExecutingSessionCount { get; }

    int CurrentExecutingTaskCount { get; }

    int TotalSchedulerSessions { get; }
  }
}
