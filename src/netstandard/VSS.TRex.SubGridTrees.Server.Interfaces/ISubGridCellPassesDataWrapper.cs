using System;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellPassesDataWrapper
  {
    IServerLeafSubGrid Owner { get; set; } // FOwner : TICServerSubGridTreeLeaf;
    ISubGridCellPassesDataSegments PassesData { get; set; }
    void Clear();
    void Initialise();
    ISubGridCellPassesDataSegment SelectSegment(DateTime time);
    bool CleaveSegment(ISubGridCellPassesDataSegment CleavingSegment);

    bool MergeSegments(ISubGridCellPassesDataSegment MergeToSegment,
      ISubGridCellPassesDataSegment MergeFromSegment);

    void RemoveSegment(ISubGridCellPassesDataSegment Segment);
  }
}
