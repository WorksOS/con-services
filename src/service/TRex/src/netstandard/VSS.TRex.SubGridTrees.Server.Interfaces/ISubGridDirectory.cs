using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridDirectory
  {
    bool ExistsInPersistentStore { get; }

    List<ISubGridCellPassesDataSegmentInfo> SegmentDirectory { get; set; }

    ISubGridCellLatestPassDataWrapper GlobalLatestCells { get; set;  }

    void AllocateGlobalLatestCells();
    void DeAllocateGlobalLatestCells();
    void CreateDefaultSegment();
    void Clear();
    bool Write(BinaryWriter writer);
    bool Read_2p0(BinaryReader reader);
  }
}
