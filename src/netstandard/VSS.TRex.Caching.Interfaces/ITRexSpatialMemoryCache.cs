using System;

namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCache
  {
    int MaxNumElements { get; }

    long MaxSizeInBytes { get; }

    int CurrentNumElements { get; }

    void ItemAddedToContext(int sizeInBytes);
    void ItemRemovedFromContext(int sizeInBytes);

    int ContextCount();

    bool Add(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element);

    ITRexSpatialMemoryCacheContext LocateOrCreateContext(string contextFingerPrint);
    ITRexSpatialMemoryCacheContext LocateOrCreateContext(string contextFingerPrint, TimeSpan cacheDuration);
  }
}
