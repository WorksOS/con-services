using System;
using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Provides a wrapper around items stored in the cache to facilitate LRU/MRU management
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct TRexCacheItem<T> where T : ITRexMemoryCacheItem
  {
    /// <summary>
    /// The item being stored in the cache
    /// </summary>
    public T Item; // No get/set semantics on purpose as this is a struct

    /// <summary>
    /// The token assigned to this item by the cache item store
    /// </summary>   
    public long MRUEpochToken; // No get/set semantics on purpose as this is a struct

    /// <summary>
    /// The time at which the cached item is no longer valid and will not be returned on a Get() call
    /// </summary>
    public DateTime ExpiryTime { get; internal set; }

    /// <summary>
    /// Determines if the cached item has hit it's expiry time
    /// </summary>
    public bool Expired => ExpiryTime < DateTime.Now;

    /// <summary>
    /// The context to which this cached item belongs
    /// </summary>
    private ITRexSpatialMemoryCacheContext Context { get; set; }

    /// <summary>
    /// Describes whether the state containing in this cache item is still valid
    /// Items are considered to be valid at the time of creation, and stay that way until explicit invalidation or
    /// overriding of the references item with null.
    /// </summary>
    public bool Valid { get; set; }

    /// <summary>
    /// The index of the previous element in the list of elements
    /// </summary>
    public int Prev; // No get/set semantics on purpose as this is a struct

    /// <summary>
    /// The index of the next element in the list of elements, or the next free entry in
    /// the list of free entries
    /// </summary>
    public int Next; // No get/set semantics on purpose as this is a struct

    public TRexCacheItem(T item, ITRexSpatialMemoryCacheContext context, long mruEpochToken, int prev, int next)
    {
      Item = item;
      Context = context;
      MRUEpochToken = mruEpochToken;
      ExpiryTime = DateTime.Now + context.CacheDurationTime;
      Prev = prev;
      Next = next;

      Valid = item != null;
    }

    public void Set(T item, ITRexSpatialMemoryCacheContext context, long mruEpochToken, int prev, int next)
    {
      Item = item;
      Context = context;
      MRUEpochToken = mruEpochToken;
      ExpiryTime = context == null ? DateTime.MinValue : DateTime.Now + context.CacheDurationTime;
      Prev = prev;
      Next = next;

      Valid = item != null;
    }

    public void GetPrevAndNext(out int prev, out int next)
    {
      prev = Prev;
      next = Next;
    }

    /// <summary>
    /// Removes this item from the context it is associated with by setting the index reference in the MRU list
    /// held in the subgrid tree to 0
    /// </summary>
    public void RemoveFromContext()
    {
      Context?.RemoveFromContextTokensOnly(Item);
    }
  }
}
