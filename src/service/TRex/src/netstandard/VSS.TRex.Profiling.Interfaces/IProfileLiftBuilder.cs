using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfileLiftBuilder<T>
  {
    bool Build(List<T> ProfileCells, ISubGridSegmentCellPassIterator cellPassIterator);
  }
}
