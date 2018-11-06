using System;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Represents a context within the overall memory cache where all the elements within the context
  /// are related, such as being members of spatial data results returned from filters with the same fingerprint
  /// </summary>
  public class TRexSpatialMemoryCacheContext : ITRexSpatialMemoryCacheContext
  {
    public ITRexSpatialMemoryCache OwnerMemoryCache { get; private set; }

    public IGenericSubGridTree_Int ContextTokens { get; private set; }

    public ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; private set; }

    private int tokenCount;
    public int TokenCount { get => tokenCount; }

    public TimeSpan CacheDurationTime { get; private set; }

    /// <summary>
    /// The default cache expiry time period for new contexts
    /// </summary>
    public static readonly TimeSpan NullCacheTimeSpan = TimeSpan.FromDays(1000);

    public TRexSpatialMemoryCacheContext(ITRexSpatialMemoryCache ownerMemoryCache,
      ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> mruList) : this(ownerMemoryCache, mruList, NullCacheTimeSpan)
    {
    }

    public TRexSpatialMemoryCacheContext(ITRexSpatialMemoryCache ownerMemoryCache,
      ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> mruList,
      TimeSpan cacheDurationTime)
    {
      ContextTokens = new GenericSubGridTree_Int(SubGridTreeConsts.SubGridTreeLevels - 1, 1);
      MRUList = mruList;
      CacheDurationTime = cacheDurationTime;
      OwnerMemoryCache = ownerMemoryCache;
    }

    /// <summary>
    /// Adds an item into a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="element"></param>
    public bool Add(ITRexMemoryCacheItem element)
    {
      lock (this)
      {
        uint x = element.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
        uint y = element.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

        // Add the element to storage and obtain its index in that storage, inserting it into the context
        // Note: The index is added as a 1-based index to the ContextTokens to differentiate iot from the null value
        // of 0 used as the null value in integer based subgrid trees
        if (ContextTokens[x, y] != 0)
        {
          // This cache element in the context already contains an item.
          // Do not overwrite the present element with the one provided
          return false;
        }

        ContextTokens[x, y] = MRUList.Add(element, this) + 1;

        tokenCount++;
        return true;
      }
    }

    /// <summary>
    /// Removes an item from a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="element"></param>
    public void Remove(ITRexMemoryCacheItem element)
    {
      uint x = element.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      uint y = element.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      lock (this)
      {

        // Locate the index for the element in the context token tree and remove it from storage,
        // null out the entry in the context token tree.
        // Note: the index in the ContextTokens tree is 1-based, so account for that in the call to Remove
        MRUList.Remove(ContextTokens[x, y] - 1);
        ContextTokens[x, y] = 0;

        tokenCount--;
      }
    }

    public ITRexMemoryCacheItem Get(uint originX, uint originY)
    {
      uint x = originX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      uint y = originY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      lock (this)
      {
        int index = ContextTokens[x, y];

        if (index == 0)
          return null;

        return MRUList.Get(index - 1);

/*        // Note: Adjust for the 1-based index obtained from ContextTokens
        var cacheItem = MRUList.Get(index - 1, out bool expired);

        if (expired)
        {
          RemoveFromContextTokensOnly(cacheItem);

          // Note: Adjust for the 1-based index obtained from ContextTokens
          MRUList.Remove(index - 1);
          return null;
        }

        return cacheItem;*/
      }
    }

    /// <summary>
    /// Removes the index for an item from the context token subgrid tree only. This is intended to be used by the MRU list to communicate
    /// elements that are being removed from the MRUList in response to adding new items to the cache.
    /// </summary>
    /// <param name="item"></param>
    public void RemoveFromContextTokensOnly(ITRexMemoryCacheItem item)
    {
      uint x = item.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      uint y = item.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      ContextTokens[x, y] = 0;
      OwnerMemoryCache.ItemRemovedFromContext(item.IndicativeSizeInBytes());
      tokenCount--;
    }
  }
}
