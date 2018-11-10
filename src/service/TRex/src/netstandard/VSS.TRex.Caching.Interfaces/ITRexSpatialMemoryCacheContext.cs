using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCacheContext
  {
    Guid ProjectUID { get; }

    string FingerPrint { get; }

    bool MarkedForRemoval { get; set; }

    DateTime MarkedForRemovalAt { get; set; }

    ITRexSpatialMemoryCache OwnerMemoryCache { get; }

    IGenericSubGridTree_Int ContextTokens { get; }

    ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; }

    int TokenCount { get; }

    TimeSpan CacheDurationTime { get; }

    TRexSpatialMemoryCacheInvalidationSensitivity Sensitivity { get; }

    bool Add(ITRexMemoryCacheItem element);

    void Remove(ITRexMemoryCacheItem element);

    ITRexMemoryCacheItem Get(uint originX, uint originY);

    void RemoveFromContextTokensOnly(ITRexMemoryCacheItem item);

    void InvalidateSubgridNoLock(uint originX, uint originY, out bool subGridPresentForInvalidation);

    void MarkForRemoval();

    void Reanimate();
  }
}
