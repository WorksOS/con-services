using System;
using System.Collections.Generic;
using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// The top level class that implements spatial data caching in TRex where that spatial data is represented by SubGrids and SubGridTrees
  /// </summary>
  public class TRexSpatialMemoryCache : ITRexSpatialMemoryCache
  {
    private const int MAX_NUM_ELEMENTS = 100000000;

    /// <summary>
    /// The MRU list that threads through all the elements in the overall cache
    /// </summary>
    public ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; private set; }
    //private MRURingBuffer<ITRexMemoryCacheItem> MRUList = null;

    private Dictionary<string, ITRexSpatialMemoryCacheContext> Contexts = null;

    public int MaxNumElements { get; set; }

    private int currentNumElements;
    public int CurrentNumElements { get => currentNumElements; }

    private long currentSizeInBytes;
    public long CurrentSizeInBytes { get => currentSizeInBytes; }

    public TRexSpatialMemoryCache(int maxNumElements, double fragmentationMultiplier, double mruDeadBandFraction)
    {
      if (maxNumElements < 1 || maxNumElements > MAX_NUM_ELEMENTS)
        throw new ArgumentException($"maxNumElements ({maxNumElements}) not in range 1..{MAX_NUM_ELEMENTS}");

      MaxNumElements = maxNumElements;

      //MRUList = new MRURingBuffer<ITRexMemoryCacheItem>(this, maxNumElements, fragmentationMultiplier, mruDeadBandFraction);
      MRUList = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(maxNumElements);
      Contexts = new Dictionary<string, ITRexSpatialMemoryCacheContext>();
    }

    /// <summary>
    /// Locates a cache context responsible for storing elements that share the same context fingerprint. If there is no matching context
    /// available then a new one is created and returned. This operation is performed under a lock covering the pool of available contexts
    /// </summary>
    /// <param name="contextFingerPrint"></param>
    /// <returns></returns>
    public ITRexSpatialMemoryCacheContext LocateOrCreateContext(string contextFingerPrint)
    {
      lock (Contexts)
      {
        if (Contexts.TryGetValue(contextFingerPrint, out ITRexSpatialMemoryCacheContext context))
          return context; // It exists, return it

        // Create the establish the new context
        ITRexSpatialMemoryCacheContext newContext = new TRexSpatialMemoryCacheContext(this, MRUList); //  new GenericSubGridTree_Long(SubGridTreeConsts.SubGridTreeLevels, 1);
        Contexts.Add(contextFingerPrint, newContext); //new GenericSubGridTree_Long(SubGridTreeConsts.SubGridTreeLevels, 1));

        return newContext;
      }
    }

    /// <summary>
    /// Adds an item into a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="element"></param>
    public void Add(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element)
    {
        context.Add(element);        
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
      System.Threading.Interlocked.Add(ref currentSizeInBytes, sizeInBytes);

      // Decrement the number of elements in the cache
      System.Threading.Interlocked.Decrement(ref currentNumElements);
    }
  }
}
