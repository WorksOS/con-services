using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Defines the storage metaphor to containing all elements committed to the cache
  /// </summary>
  public class TRexSpatialMemoryCacheStorage<T> : ITRexSpatialMemoryCacheStorage<T>
  {
    private TRexCacheItem<T>[] Items;

    private int LRUHead;
    private int FreeListHead;

    private long CurrentToken = -1;
    private long NextToken => System.Threading.Interlocked.Increment(ref CurrentToken);

    private int tokenCount = 0;
    public int TokenCount { get => tokenCount; }

    public TRexSpatialMemoryCacheStorage(int maxNumElements)
    {
      // Allocate all the wrapper for the cached items into a single array
      Items = new TRexCacheItem<T>[maxNumElements];

      // Initialise the LRU head to -1 (ie: no items in the list)
      LRUHead = -1;

      // Initialise all items to be within the free list
      FreeListHead = 0;
      for (int i = 0; i < maxNumElements - 1; i++)
        Items[i].Next = i + 1;
      Items[maxNumElements - 1].Next = -1;

      for (int i = 0; i < maxNumElements; i++)
        Items[i].Prev = i - 1;
    }

    /// <summary>
    /// Evicts a single item from the LRU list (the oldest) and joins it to the free list
    /// Note: This method does not independently lock the list, the caller is responsible for required locking.
    /// </summary>
    private void EvictOneLRUItemNoLock()
    {
      int oldLRUHead = LRUHead;
      LRUHead = Items[LRUHead].Next;
      Items[LRUHead].Prev = -1;

      Items[FreeListHead].Prev = oldLRUHead;
      Items[oldLRUHead].Next = FreeListHead;

      FreeListHead = oldLRUHead;
    }

    /// <summary>
    /// Adds an item into the cache storage.
    /// </summary>
    /// <param name="element"></param>
    /// <returns>The index of the newly added item</returns>
    public int Add(T element)
    {
      int index;
      long token = NextToken;

      lock (this)
      {
        // Obtain item from free list
        if (FreeListHead == -1)
        {
          // There are no free entries, victimize one to store it
          EvictOneLRUItemNoLock();
        }

        index = FreeListHead;

        FreeListHead = Items[index].Next;
        Items[index].Set(element, token, -1, LRUHead);

        Items[LRUHead].Prev = index;
        LRUHead = index;

        tokenCount++;
      }

      // Return the token to the caller
      return index;
    }

    /// <summary>
    /// Removes an item from storage given its index
    /// </summary>
    /// <param name="index"></param>
    public int Remove(int index)
    {
      lock (this)
      {
        Items[index].GetPrevAndNext(out int prev, out int next);

        if (prev != -1)
          Items[prev].Next = next;

        if (next != -1)
          Items[next].Prev = prev;

        Items[index].Set(default(T), -1, -1, FreeListHead);
        FreeListHead = index;

        tokenCount--;
        return -1;
      }
    }
  }
}
