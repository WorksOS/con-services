using System;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching
{
  public interface ITRexSpatialMemoryCacheContext
  {
    string FingerPrint { get; }

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
  }
}
