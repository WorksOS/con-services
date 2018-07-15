using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfileLiftBuilder
  {
    bool Build(List<ProfileCell> ProfileCells, ISubGridSegmentCellPassIterator cellPassIterator);
  }
}
