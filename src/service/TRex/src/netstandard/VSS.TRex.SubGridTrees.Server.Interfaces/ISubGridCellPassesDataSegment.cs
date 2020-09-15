using System;
using System.IO;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellPassesDataSegment : IDisposable
  {
    IServerLeafSubGrid Owner { get; }

    /// <summary>
    /// Tracks whether there are unsaved changes in this segment
    /// </summary>
    bool Dirty { get; set; }

    bool HasAllPasses { get; }
    bool HasLatestData { get; }
    ISubGridCellPassesDataSegmentInfo SegmentInfo { get; set; }
    ISubGridCellSegmentPassesDataWrapper PassesData { get; }
    ISubGridCellLatestPassDataWrapper LatestPasses { get; }

    /// <summary>
    /// Determines if this segments time range bounds the data time given in the time argument
    /// </summary>
    bool SegmentMatches(DateTime time);

    void AllocateFullPassStacks();
    void AllocateLatestPassGrid();
    void DeAllocateFullPassStacks();
    void DeAllocateLatestPassGrid();
    bool SavePayloadToStream(BinaryWriter writer);

    bool LoadPayloadFromStream(BinaryReader reader,
      bool loadLatestData,
      bool loadAllPasses);

    bool Read(BinaryReader reader,
      bool loadLatestData, bool loadAllPasses);

    bool Write(BinaryWriter writer);

    bool SaveToFile(IStorageProxy storage,
      string fileName,
      out FileSystemErrorStatus fsError);

    /// <summary>
    /// Determines if this segment violates either the maximum number of cell passes within a 
    /// segment limit, or the maximum number of cell passes within a single cell within a
    /// segment limit.
    /// If either limit is breached, this segment requires cleaving
    /// </summary>
    bool RequiresCleaving(out int totalPasses, out int maxPassCount);

    /// <summary>
    /// Verifies if the segment time range bounds are consistent with the cell passes it contains
    /// </summary>
    bool VerifyComputedAndRecordedSegmentTimeRangeBounds();
  }
}
