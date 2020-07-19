using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface IServerSubGridTree : ISubGridTree
  {
    bool IsMutable { get; }

    bool LoadLeafSubGridSegment(IStorageProxy storageProxy,
      SubGridCellAddress cellAddress,
      bool loadLatestData,
      bool loadAllPasses,
      IServerLeafSubGrid subGrid,
      ISubGridCellPassesDataSegment segment);

    bool LoadLeafSubGrid(IStorageProxy storageProxy,
      SubGridCellAddress cellAddress,
      bool loadAllPasses, bool loadLatestPasses,
      IServerLeafSubGrid subGrid);

    bool SaveLeafSubGrid(IServerLeafSubGrid subGrid,
      IStorageProxy storageProxyForSubGrids,
      IStorageProxy storageProxyForSubGridSegments,
      List<ISubGridSpatialAffinityKey> invalidatedSpatialStreams);

    ServerSubGridTreeCachingStrategy CachingStrategy { get; set; }
  }
}
