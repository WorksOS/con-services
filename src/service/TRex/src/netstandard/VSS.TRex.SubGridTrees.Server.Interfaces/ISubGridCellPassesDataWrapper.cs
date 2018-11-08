using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellPassesDataWrapper
  {
    IServerLeafSubGrid Owner { get; set; } 
    ISubGridCellPassesDataSegments PassesData { get; set; }
    void Clear();
    void Initialise();
    ISubGridCellPassesDataSegment SelectSegment(DateTime time);
    bool CleaveSegment(ISubGridCellPassesDataSegment CleavingSegment, 
      List<ISubGridSpatialAffinityKey> PersistedClovenSegments);

    bool MergeSegments(ISubGridCellPassesDataSegment MergeToSegment,
      ISubGridCellPassesDataSegment MergeFromSegment);

    void RemoveSegment(ISubGridCellPassesDataSegment Segment);
  }
}
