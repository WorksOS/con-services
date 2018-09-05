using System;
using System.IO;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellPassesDataSegment
  {
    ISubGrid Owner { get; set; }

    /// <summary>
    /// Tracks whether there are unsaved changes in this segment
    /// </summary>
    bool Dirty { get; set; }

    bool HasAllPasses { get; set; }
    bool HasLatestData { get; set; }
    ISubGridCellPassesDataSegmentInfo SegmentInfo { get; set; }
    ISubGridCellSegmentPassesDataWrapper PassesData { get; set; }
    ISubGridCellLatestPassDataWrapper LatestPasses { get; set; }

    /// <summary>
    /// Determines if this segments tiume range bounds the data tiem givein in the time argument
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    bool SegmentMatches(DateTime time);

    void AllocateFullPassStacks();
    void AllocateLatestPassGrid();
    void DeAllocateFullPassStacks();
    void DeAllocateLatestPassGrid();
    bool SavePayloadToStream(BinaryWriter writer);

    bool LoadPayloadFromStream_v2p0(BinaryReader reader,
      bool loadLatestData,
      bool loadAllPasses);

    bool Read(BinaryReader reader,
      bool loadLatestData, bool loadAllPasses);

    bool Write(BinaryWriter writer);
    void CalculateElevationRangeOfPasses();

    bool SaveToFile(IStorageProxy storage,
      string FileName,
      out FileSystemErrorStatus FSError);

    /// <summary>
    /// Determines if this segment violates either the maximum number of cell passes within a 
    /// segment limit, or the maximum numebr of cell passes within a single cell within a
    /// segment limit.
    /// If either limit is breached, this segment requires cleaving
    /// </summary>
    /// <returns></returns>
    bool RequiresCleaving();

    /// <summary>
    /// Verifies if the segment time range bounds are consistent with the cell passes it contains
    /// </summary>
    /// <returns></returns>
    bool VerifyComputedAndRecordedSegmentTimeRangeBounds();
  }
}
