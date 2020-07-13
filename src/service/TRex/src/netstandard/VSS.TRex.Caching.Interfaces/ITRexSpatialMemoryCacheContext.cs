using System;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCacheContext
  {
    Guid ProjectUID { get; }

    string FingerPrint { get; }

    GridDataType GridDataType { get; }

    bool MarkedForRemoval { get; set; }

    DateTime MarkedForRemovalAtUtc { get; set; }

    ITRexSpatialMemoryCache OwnerMemoryCache { get; }

    IGenericSubGridTree_Int ContextTokens { get; }

    ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; }

    int TokenCount { get; }

    TimeSpan CacheDurationTime { get; }

    TRexSpatialMemoryCacheInvalidationSensitivity Sensitivity { get; }

    bool Add(ITRexMemoryCacheItem element);

    void Remove(ITRexMemoryCacheItem element);

    ITRexMemoryCacheItem Get(int originX, int originY);

    void RemoveFromContextTokensOnly(ITRexMemoryCacheItem item);

    void InvalidateSubGridNoLock(int originX, int originY, out bool subGridPresentForInvalidation);

    void InvalidateAllSubGridsNoLock();

    void MarkForRemoval(DateTime markedForRemovalAtUtc);

    void Reanimate();
  }
}
