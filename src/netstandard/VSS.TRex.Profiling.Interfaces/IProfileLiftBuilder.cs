using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfileLiftBuilder
  {
    bool Build(List<IProfileCell> ProfileCells, ISubGridSegmentCellPassIterator cellPassIterator);
  }
}
