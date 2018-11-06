using System;
using System.Collections.Generic;
using System.Diagnostics;
using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// The top level class that implements spatial data caching in TRex where that spatial data is represented by SubGrids and SubGridTrees
  /// </summary>
  public class TRexSpatialMemoryCache : ITRexSpatialMemoryCache
  {
    private const int MAX_NUM_ELEMENTS = 1000000000;

    /// <summary>
    /// The MRU list that threads through all the elements in the overall cache
    /// </summary>
    public ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; }

    private readonly Dictionary<string, ITRexSpatialMemoryCacheContext> Contexts;

    // ReSharper disable once InconsistentlySynchronizedField
    public int ContextCount() => Contexts.Count;

    public int MaxNumElements { get; }

    private int currentNumElements;
    public int CurrentNumElements => currentNumElements;

    private long currentSizeInBytes;
    public long CurrentSizeInBytes => currentSizeInBytes;

    public int MruNonUpdateableSlotCount { get; }

    public long MaxSizeInBytes { get; }

    /// <summary>
    /// Creates a new spatial data cache containing at most maxNumElements items. Elements are stored in
    /// an MRU list and are moved to the top of the MRU list of their distance from the top of the list at the time they
    /// are touched is outside the MRU dead band (expressed as a fraction of the overall maximum number of elements in the cache.
    /// </summary>
    /// <param name="maxNumElements"></param>
    /// <param name="maxSizeInBytes"></param>
    /// <param name="mruDeadBandFraction"></param>
    public TRexSpatialMemoryCache(int maxNumElements, long maxSizeInBytes, double mruDeadBandFraction)
    {
      if (maxNumElements < 1 || maxNumElements > MAX_NUM_ELEMENTS)
        throw new ArgumentException($"maxNumElements ({maxNumElements}) not in range 1..{MAX_NUM_ELEMENTS}");

      // Set cache size range between 1kb and 100Gb
      if (maxSizeInBytes < 1000 || maxSizeInBytes > 100000000000)
        throw new ArgumentException($"maxSizeInBytes ({maxSizeInBytes}) not in range 1000..100000000000 (1e3..1e11)");

      if (mruDeadBandFraction < 0.0 || mruDeadBandFraction > 1.0)
        throw new ArgumentException($"mruDeadBandFraction ({mruDeadBandFraction}) not in range 0.0..1.0");

      MaxNumElements = maxNumElements;
      MaxSizeInBytes = maxSizeInBytes;
      MruNonUpdateableSlotCount = (int)Math.Truncate(maxNumElements * mruDeadBandFraction);

      MRUList = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(maxNumElements, MruNonUpdateableSlotCount);
      Contexts = new Dictionary<string, ITRexSpatialMemoryCacheContext>();
    }

    /// <summary>
    /// Locates a cache context responsible for storing elements that share the same context fingerprint. If there is no matching context
    /// available then a new one is created and returned. This operation is performed under a lock covering the pool of available contexts
    /// </summary>
    /// <param name="contextFingerPrint"></param>
    /// <param name="cacheDuration"></param>
    /// <returns></returns>
    public ITRexSpatialMemoryCacheContext LocateOrCreateContext(string contextFingerPrint, TimeSpan cacheDuration)
    {
      lock (Contexts)
      {
        if (Contexts.TryGetValue(contextFingerPrint, out ITRexSpatialMemoryCacheContext context))
          return context; // It exists, return it

        // Create the establish the new context
        ITRexSpatialMemoryCacheContext newContext = new TRexSpatialMemoryCacheContext(this, MRUList, cacheDuration); 
        Contexts.Add(contextFingerPrint, newContext);

        return newContext;
      }
    }

    /// <summary>
    /// Locates a cache context responsible for storing elements that share the same context fingerprint. If there is no matching context
    /// available then a new one is created and returned. This operation is performed under a lock covering the pool of available contexts
    /// </summary>
    /// <param name="contextFingerPrint"></param>
    /// <returns></returns>
    public ITRexSpatialMemoryCacheContext LocateOrCreateContext(string contextFingerPrint)
    {
      return LocateOrCreateContext(contextFingerPrint, TRexSpatialMemoryCacheContext.NullCacheTimeSpan);
    }

    /// <summary>
    /// Adds an item into a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="element"></param>
    public bool Add(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element)
    {
      bool result = context.Add(element);

      // Perform some house keeping to keep the cache size in bounds
      ItemAddedToContext(element.IndicativeSizeInBytes());
      while (CurrentSizeInBytes > MaxSizeInBytes)
      {
        MRUList.EvictOneLRUItemWithLock();
      }

      return result;
    }

    /// <summary>
    /// Removes an item from a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="element"></param>
    public void Remove(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element)
    {
        context.Remove(element);

        // Perform some house keeping to keep the cache size in bounds
        ItemRemovedFromContext(element.IndicativeSizeInBytes());
    }

    public void ItemAddedToContext(int sizeInBytes)
    {
      // Increment the number of elements in the cache
      System.Threading.Interlocked.Increment(ref currentNumElements);

      // Increment the memory usage in the cache
      System.Threading.Interlocked.Add(ref currentSizeInBytes, sizeInBytes);
    }

    public void ItemRemovedFromContext(int sizeInBytes)
    {
      // Decrement the memory usage in the cache
      var number = System.Threading.Interlocked.Add(ref currentSizeInBytes, -sizeInBytes);

      if (number < 0)
      {
        Debug.Assert(false, "CurrentSizeInBytes < 0! Consider using Cache.Add(context, item).");
      }

      // Decrement the number of elements in the cache
      System.Threading.Interlocked.Decrement(ref currentNumElements);
    }

    /// <summary>
    /// Attempts to read an element from a cache context given the spatial location of the element
    /// </summary>
    /// <param name="context">The request, filter and other data specific context for spatial data</param>
    /// <param name="originX">The origin (bottom left) cell of the spatial data subgrid</param>
    /// <param name="originY">The origin (bottom left) cell of the spatial data subgrid</param>
    /// <returns></returns>
    public ITRexMemoryCacheItem Get(ITRexSpatialMemoryCacheContext context, uint originX, uint originY)
    {
      return context.Get(originX, originY);
    }
  }
}
