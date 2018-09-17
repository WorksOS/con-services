using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridDirectory
  {
    List<ISubGridCellPassesDataSegmentInfo> SegmentDirectory { get; set; }
    ISubGridCellLatestPassDataWrapper GlobalLatestCells { get; set; }

    /// <summary>
    /// Adds a segment to the persistent list of cloven segments. The underlying list is created
    /// on demand under a subgrid lock
    /// </summary>
    /// <param name="segment"></param>
    void AddPersistedClovenSegment(ISubGridCellPassesDataSegmentInfo segment);

    /// <summary>
    /// Extracts and returns the current list of persisted cloven segments. THe internal list is set to null
    /// </summary>
    /// <returns></returns>
    List<ISubGridCellPassesDataSegmentInfo> ExtractPersistedClovenSegments();

    void AllocateGlobalLatestCells();
    void DeAllocateGlobalLatestCells();
    void CreateDefaultSegment();
    void Clear();
    bool Write(BinaryWriter writer);
    bool Read_2p0(BinaryReader reader);
  }
}
