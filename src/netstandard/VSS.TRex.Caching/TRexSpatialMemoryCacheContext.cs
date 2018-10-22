using System;
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
    public IGenericSubGridTree_Long ContextTokens { get; private set; }

    public IMRURingBuffer<ITRexMemoryCacheItem> MRUList { get; private set; }

    public int TokenCount { get; private set; }

    public TRexSpatialMemoryCacheContext(IMRURingBuffer<ITRexMemoryCacheItem> mruList)
    {
      TokenCount = 0;
      ContextTokens = new GenericSubGridTree_Long(SubGridTreeConsts.SubGridTreeLevels, 1);
      MRUList = mruList;
    }

    /// <summary>
    /// Adds an item into a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="element"></param>
    public void Add(ITRexMemoryCacheItem element)
    {
      // Determine if adding the item will violate the max num elements constraint
      // ...

      // If so, then remove the LRU item from the cache to make room for it
      // ...

      // Determine if adding the item will violate the maximum size constraint
      // ...

      // Add the element to the ring buffer, obtaining it's token
      // ...

      // Insert the token obtained from the ring buffer into the context
      // ...

      throw new NotImplementedException();
    }

    /// <summary>
    /// Removes an item from a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="element"></param>
    public void Remove(ISubGrid element)
    {
      // Locate the ring buffer token for this element from the context
      //...

      // Instruct the ring buffer to release the element at the token
      //...

      // Remove the token from the context
      //...

      throw new NotImplementedException();
    }
  }
}
