namespace VSS.TRex.Caching
{
  /// <summary>
  /// Provides a wrapper around items stored in the cache to facilitate LRU/MRU management
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct TRexCacheItem<T>
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
    /// The index of the previous element in the list of elements
    /// </summary>
    public int Prev; // No get/set semantics on purpose as this is a struct

    /// <summary>
    /// The index of the next element in the list of elements, or the next free entry in
    /// the list of free entries
    /// </summary>
    public int Next; // No get/set semantics on purpose as this is a struct

    public TRexCacheItem(T item, long mruEpochToken, int prev, int next)
    {
      Item = item;
      MRUEpochToken = mruEpochToken;
      Prev = prev;
      Next = next;
    }

    public void Set(T item, long mruEpochToken, int prev, int next)
    {
      Item = item;
      MRUEpochToken = mruEpochToken;
      Prev = prev;
      Next = next;
    }

    public void GetPrevAndNext(out int prev, out int next)
    {
      prev = Prev;
      next = Next;
    }
  }
}
