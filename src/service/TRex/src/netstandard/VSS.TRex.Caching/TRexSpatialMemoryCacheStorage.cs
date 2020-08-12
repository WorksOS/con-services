using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Defines the storage metaphor to containing all elements committed to the cache
  /// </summary>
  public class TRexSpatialMemoryCacheStorage<T> : ITRexSpatialMemoryCacheStorage<T> where T : ITRexMemoryCacheItem
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<TRexSpatialMemoryCacheStorage<T>>();

    private readonly TRexCacheItem<T>[] _items;

    /// <summary>
    /// The head of the most recently used list. MRUHead is the most recently added or modified element in the list. 
    /// Traverse it with *.Next to get progressively older elements
    /// </summary>
    public int MRUHead { get; private set; }

    /// <summary>
    /// The head of the least recently used list. LRUHead is the least recently added or modified element in the list
    /// Traverse it with *.Prev to get progressively older elements
    /// </summary>
    public int LRUHead { get; private set; }

    private int _freeListHead;

    private long _currentToken = -1;
    private long NextToken() => System.Threading.Interlocked.Increment(ref _currentToken);

    private readonly int _maxMruEpochTokenAge;

    private int _tokenCount;

    // ReSharper disable once InconsistentlySynchronizedField
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int TokenCount => _tokenCount;

    public bool HasFreeSpace() => _freeListHead != -1;

    /// <summary>
    /// Constructs a storage ring to contain a fixed maximum number of elements in the cache. The ring defines two internal
    /// doubly linked lists, one to define the MRU list of elements in the ring, and the other to define the list of slots
    /// in the ring that are unoccupied, or free.
    /// </summary>
    /// <param name="maxNumElements"></param>
    /// <param name="maxMruEpochTokenAge"></param>
    public TRexSpatialMemoryCacheStorage(int maxNumElements, int maxMruEpochTokenAge)
    {
      // Allocate all the wrapper for the cached items into a single array
      _items = new TRexCacheItem<T>[maxNumElements];

      // Initialise the MRU head to -1 (ie: no items in the list)
      MRUHead = -1;
      LRUHead = -1;

      // Initialise all items to be within the free list
      _freeListHead = 0;
      for (var i = 0; i < maxNumElements - 1; i++)
        _items[i].Next = i + 1;
      _items[maxNumElements - 1].Next = -1;

      for (var i = 0; i < maxNumElements; i++)
        _items[i].Prev = i - 1;

      _maxMruEpochTokenAge = maxMruEpochTokenAge;
    }

    /// <summary>
    /// Evicts a single item from the LRU list (the oldest) and joins it to the free list
    /// Note: This method does not independently lock the list, the caller is responsible for required locking.
    /// </summary>
    public void EvictOneLRUItem()
    {
      if (LRUHead == -1)
        return;

      var oldLRUHead = LRUHead;
      LRUHead = _items[LRUHead].Prev;

      if (_freeListHead != -1)
      {
        _items[_freeListHead].Prev = oldLRUHead;

        _items[oldLRUHead].Next = _freeListHead;
        _items[oldLRUHead].Prev = -1;
      }
      else
      {
        _items[oldLRUHead].Next = -1;
        _items[oldLRUHead].Prev = -1;
      }

      _freeListHead = oldLRUHead;

      // Set the index in the context to the element just evicted to zero
      _items[_freeListHead].RemoveFromContext();

      // Adjust the token count in the MRU list
      _tokenCount--;
    }

    /// <summary>
    /// Adds an item into the cache storage.
    /// </summary>
    /// <returns>The index of the newly added item</returns>
    public int Add(T element, ITRexSpatialMemoryCacheContext context)
    {
      var token = NextToken();

      // Obtain item from free list
      if (_freeListHead == -1)
      {
        // There are no free entries, victimize one to store it
        EvictOneLRUItem();
      }

      var index = _freeListHead;

      _freeListHead = _items[index].Next;

      // Set the parameters for the new item, setting it's prev pointer to point to the oldest member of the MRUList
      if (MRUHead == -1)
      {
        _items[index].Set(element, context, token, index, MRUHead);
      }
      else
      {
        _items[index].Set(element, context, token, _items[MRUHead].Prev, MRUHead);
        _items[MRUHead].Prev = index;
      }

      if (LRUHead == -1)
      {
        LRUHead = index;
      }

      MRUHead = index;

      _tokenCount++;

      // Return the token to the caller
      return index;
    }

    /// <summary>
    /// Removes an item from storage given its index
    /// </summary>
    public void Remove(int index)
    {
      _items[index].GetPrevAndNext(out var prev, out var next);

      if (prev != -1)
        _items[prev].Next = next;

      if (next != -1)
        _items[next].Prev = prev;

      _items[index].Set(default, null, -1, -1, _freeListHead);
      _freeListHead = index;

      _tokenCount--;
    }

    /// <summary>
    /// Moves the element at the index location in the element storage so that it is now the most recently
    /// used element in the cache. This is done by modifying the Prev and Next references in the doubly linked list.
    /// Note: The location of the item in the list is not moved as a result of this, so all external indexes relating
    /// to it continue to be valid.
    /// </summary>
    private void TouchItem(int index)
    {
      // Save the indexes of the previous and next items
      _items[index].GetPrevAndNext(out var prev, out var next);

      // Rewire previous and next references in the neighbors to cut this item out of the linked list
      if (prev != -1)
        _items[prev].Next = next;
      if (next != -1)
        _items[next].Prev = prev;

      // Add the current item to the MRUHead
      _items[index].Prev = -1;
      _items[index].Next = MRUHead;

      // Update MRUHead to point to item at the head of the list
      MRUHead = index;
    }

    /// <summary>
    /// Retrieves the cached item from the specified index in the MRU list
    /// If the element present in the MRU list is Expired or not Valid it
    /// is proactively removed and null is returned to the caller.
    /// </summary>
    public T Get(int index)
    {
      var cacheItem = _items[index];

      if (cacheItem.Context == null)
      {
        // This element has no home, it is by definition null
        return default;
      }

      if (cacheItem.Expired)
      {
        cacheItem.Context.OwnerMemoryCache.Remove(cacheItem.Context, cacheItem.Item);
        return default;
      }

      if (_currentToken - _items[index].MRUEpochToken > _maxMruEpochTokenAge)
      {
        TouchItem(index);

        // Advance the current token so all elements 'age' by one
        NextToken();
      }

      return cacheItem.Item;
    }

    public bool IsEmpty() => MRUHead == -1;
  }
}
