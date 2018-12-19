using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellProfileAnalyzer<T>
  {
    bool Analyze(List<T> ProfileCells, ISubGridSegmentCellPassIterator cellPassIterator);
  }
}
