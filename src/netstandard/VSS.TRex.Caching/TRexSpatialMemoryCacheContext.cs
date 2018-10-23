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
    private ITRexSpatialMemoryCache OwnerMemoryCache;

    public IGenericSubGridTree_Int ContextTokens { get; private set; }

   // public IMRURingBuffer<ITRexMemoryCacheItem> MRUList { get; private set; }

    public ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; private set; }

    private int tokenCount = 0;
    public int TokenCount { get => tokenCount; }

    public TRexSpatialMemoryCacheContext(ITRexSpatialMemoryCache ownerMemoryCache,
                                         ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> mruList)
    {
      ContextTokens = new GenericSubGridTree_Int(SubGridTreeConsts.SubGridTreeLevels, 1);
      MRUList = mruList;
      OwnerMemoryCache = ownerMemoryCache;
    }

    /// <summary>
    /// Adds an item into a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="element"></param>
    public void Add(ITRexMemoryCacheItem element)
    {
      lock (this)
      {
        OwnerMemoryCache.ItemRemovedFromContext(element.IndicativeSizeInBytes());

        uint x = element.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
        uint y = element.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

        // Add the element to storage and obtain its index in that storage, inserting it into the context
        ContextTokens[x, y] = MRUList.Add(element);

        tokenCount++;
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
      uint x = element.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      uint y = element.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      lock (this)
      {
        OwnerMemoryCache.ItemAddedToContext(element.IndicativeSizeInBytes());

        // Locate the index for the element in the context token tree and remove it from storage,
        // nulling out the entry in the context token tree
        ContextTokens[x, y] = MRUList.Remove(ContextTokens[x, y]);

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

        return index != -1 ? MRUList.Get(index) : null;
      }
    }
  }
}
