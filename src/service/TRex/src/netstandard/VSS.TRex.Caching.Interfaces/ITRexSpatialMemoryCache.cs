using System;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCache
  {
    int MaxNumElements { get; }

    long MaxSizeInBytes { get; }

    long CurrentSizeInBytes { get; }

    int CurrentNumElements { get; }

    void ItemRemovedFromContext(int sizeInBytes);

    int ContextCount { get; }
    int ProjectCount { get; }

    long ContextRemovalCount { get; }

    CacheContextAdditionResult Add(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element, long invalidationVersion);
    ITRexMemoryCacheItem Get(ITRexSpatialMemoryCacheContext context, int originX, int originY);
    void Remove(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element);

    ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, GridDataType gridDataType, string contextFingerPrint);
    ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, GridDataType gridDataType, string contextFingerPrint, TimeSpan cacheDuration);

    void InvalidateDueToProductionDataIngest(Guid projectUid, ISubGridTreeBitMask mask);
    void InvalidateDueToDesignChange(Guid projectUid, Guid designUid);

    void RemoveContextsMarkedForRemoval(int ageSeconds);
  }
}
