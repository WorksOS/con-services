using System.Collections.Generic;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellPassesDataSegments
  {
    List<ISubGridCellPassesDataSegment> Items { get; set; }
    int Count { get; }
    void Clear();
    ISubGridCellPassesDataSegment this[int index] { get; }
    int Add(ISubGridCellPassesDataSegment item);

    ISubGridCellPassesDataSegment AddNewSegment(IServerLeafSubGrid subGrid,
      ISubGridCellPassesDataSegmentInfo segmentInfo);
  }
}
