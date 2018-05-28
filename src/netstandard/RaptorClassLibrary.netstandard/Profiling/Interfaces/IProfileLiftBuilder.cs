using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfileLiftBuilder
  {
    bool Build(ISubGridSegmentCellPassIterator cellPassIterator);
  }
}
