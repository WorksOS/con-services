using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCache
  {
    int MaxNumElements { get; }

    long MaxSizeInBytes { get; }

    long CurrentSizeInBytes { get; }

    int CurrentNumElements { get; }

    //void ItemAddedToContext(int sizeInBytes);
    void ItemRemovedFromContext(int sizeInBytes);

    int ContextCount { get; }
    int ProjectCount { get; }

    long ContextRemovalCount { get; }

    bool Add(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element);
    ITRexMemoryCacheItem Get(ITRexSpatialMemoryCacheContext context, uint originX, uint originY);
    void Remove(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element);

    ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, string contextFingerPrint);
    ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, string contextFingerPrint, TimeSpan cacheDuration);

    void InvalidateDueToProductionDataIngest(Guid projectUid, ISubGridTreeBitMask mask);

    void RemoveContextsMarkedForRemoval(TimeSpan age);
  }
}
