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
  }
}
