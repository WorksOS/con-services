using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellProfileAnalyzer<T>
  {
    Task<bool> Analyze(List<T> ProfileCells, ISubGridSegmentCellPassIterator cellPassIterator);
  }
}
