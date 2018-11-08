using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface IServerSubGridTree : ISubGridTree
  {
        bool LoadLeafSubGridSegment(IStorageProxy StorageProxy,
                                    SubGridCellAddress cellAddress,
                                    bool loadLatestData,
                                    bool loadAllPasses,
                                    IServerLeafSubGrid SubGrid,
                                    ISubGridCellPassesDataSegment Segment);

    bool LoadLeafSubGrid(IStorageProxy storageProxy,
      SubGridCellAddress CellAddress,
      bool loadAllPasses, bool loadLatestPasses,
      IServerLeafSubGrid SubGrid);

    bool SaveLeafSubGrid(IServerLeafSubGrid subGrid, IStorageProxy storageProxy, List<ISubGridSpatialAffinityKey> invalidatedSpatialStreams);
  }
}
