using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Defines the storage metaphor to containing all elements committed to the cache
  /// </summary>
  public class TRexSpatialMemoryCacheStorage<T> : ITRexSpatialMemoryCacheStorage<T> where T : ITRexMemoryCacheItem
  {
    private TRexCacheItem<T>[] Items;

    private int MRUHead;
    private int FreeListHead;

    private long CurrentToken = -1;
    private long NextToken => System.Threading.Interlocked.Increment(ref CurrentToken);

    private int tokenCount = 0;

    private int MaxMRUEpochTokenAge;

    public int TokenCount { get => tokenCount; }

    public bool HasFreeSpace() => FreeListHead != -1;

    /// <summary>
    /// Constructs a storage ring to contain a fixed maximum number of elements in the cache. The ring defines two internal
    /// doubly linked lists, one to define the MRU list of elements in the ring, and the other to define the list of slots
    /// in the ring that are unoccupied, or free.
    /// </summary>
    /// <param name="maxNumElements"></param>
    /// <param name="maxMRUEpochTokenAge"></param>
    public TRexSpatialMemoryCacheStorage(int maxNumElements, int maxMRUEpochTokenAge)
    {
      // Allocate all the wrapper for the cached items into a single array
      Items = new TRexCacheItem<T>[maxNumElements];

      // Initialise the MRU head to -1 (ie: no items in the list)
      MRUHead = -1;

      // Initialise all items to be within the free list
      FreeListHead = 0;
      for (int i = 0; i < maxNumElements - 1; i++)
        Items[i].Next = i + 1;
      Items[maxNumElements - 1].Next = -1;

      for (int i = 0; i < maxNumElements; i++)
        Items[i].Prev = i - 1;

      MaxMRUEpochTokenAge = maxMRUEpochTokenAge;
    }

    /// <summary>
    /// Evicts a single item from the LRU list (the oldest) and joins it to the free list
    /// Note: This method does not independently lock the list, the caller is responsible for required locking.
    /// </summary>
    private void EvictOneLRUItemNoLock()
    {
      if (MRUHead == -1)
        return;

      int LRUHead = Items[MRUHead].Prev;
      Items[MRUHead].Prev = Items[LRUHead].Prev;

      MRUHead = Items[MRUHead].Next;
      Items[LRUHead].Prev = -1;

      if (FreeListHead != -1)
      {
        Items[FreeListHead].Prev = LRUHead;
        Items[LRUHead].Next = FreeListHead;
      }
      else
      {
        Items[LRUHead].Next = -1;
      }

      FreeListHead = LRUHead;

      // Set the index in the context to the element just evicted to zero
      Items[FreeListHead].RemoveFromContext();

      // Adjust the token count in the MRU list
      tokenCount--;
    }

    /// <summary>
    /// Adds an item into the cache storage.
    /// </summary>
    /// <param name="element"></param>
    /// <returns>The index of the newly added item</returns>
    public int Add(T element, ITRexSpatialMemoryCacheContext context)
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

        // Set the parameters for the new item, setting it's prev pointer to point to the oldest member of the MRUList
        if (MRUHead == -1)
          Items[index].Set(element, context, token, index, MRUHead);
        else
        {
          Items[index].Set(element, context, token, Items[MRUHead].Prev, MRUHead);
          Items[MRUHead].Prev = index;
        }

        MRUHead = index;

        tokenCount++;
      }

      // Return the token to the caller
      return index;
    }

    /// <summary>
    /// Removes an item from storage given its index
    /// </summary>
    /// <param name="index"></param>
    public void Remove(int index)
    {
      lock (this)
      {
        Items[index].GetPrevAndNext(out int prev, out int next);

        if (prev != -1)
          Items[prev].Next = next;

        if (next != -1)
          Items[next].Prev = prev;

        Items[index].Set(default(T), null, -1, -1, FreeListHead);
        FreeListHead = index;

        tokenCount--;
      }
    }

    /// <summary>
    /// Moves the element at the index location in the element storage so that it is now the most recently
    /// used element in the cache. This is done by modifying the Prev and Next references in the doubly linked list.
    /// Note: The location of the item in the list is not moved as a result of this, so all external indexes relating
    /// to it continue to be valid.
    /// </summary>
    /// <param name="index"></param>
    private void TouchItemNoLock(int index)
    {
      // Save the indexes of the previous and next items
      Items[index].GetPrevAndNext(out int prev, out int next);

      // Rewire previous and next references in the neighbors to cut this item out of the linked list
      if (prev != -1)
        Items[prev].Next = next;
      if (next != -1)
        Items[next].Prev = prev;

      // Add the current item to the MRUHead
      Items[index].Prev = -1;
      Items[index].Next = MRUHead;

      // Update MRUHead to point to item at the head of the list
      MRUHead = index;
    }

    /// <summary>
    /// Retrieves the cached item from the specified index in the MRU list
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T Get(int index)
    {
      lock (this)
      {
        if (CurrentToken - Items[index].MRUEpochToken < MaxMRUEpochTokenAge)
          TouchItemNoLock(index);

        return Items[index].Item;
      }
    }
  }
}
