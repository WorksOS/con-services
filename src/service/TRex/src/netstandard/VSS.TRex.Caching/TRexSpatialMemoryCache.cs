using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// The top level class that implements spatial data caching in TRex where that spatial data is represented by SubGrids and SubGridTrees
  /// </summary>
  public class TRexSpatialMemoryCache : ITRexSpatialMemoryCache
  {
    private static readonly ILogger log = Logging.Logger.CreateLogger<TRexSpatialMemoryCache>();

    private const int MAX_NUM_ELEMENTS = 1000000000;

    /// <summary>
    /// The MRU list that threads through all the elements in the overall cache
    /// </summary>
    public ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; }

    /// <summary>
    /// The collection of contexts managed within the overall memory cache.
    /// Each context is identified by a string based fingerprint that encodes information such
    /// as project, grid data type requests & filter parameters etc
    /// </summary>
    private readonly Dictionary<string, ITRexSpatialMemoryCacheContext> Contexts;

    /// <summary>
    /// A collection of cache context lists, one per project active in the system.
    /// This provides a single clear boundary for locating all cache contexts related
    /// to a project for operations such as invalidation due to TAG file ingest and
    /// eviction of contexts that are empty or are otherwise subject to removal.
    /// </summary>
    private readonly Dictionary<Guid, List<ITRexSpatialMemoryCacheContext>> ProjectContexts;

    // ReSharper disable once InconsistentlySynchronizedField
    public int ContextCount => Contexts.Count;

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
      ProjectContexts = new Dictionary<Guid, List<ITRexSpatialMemoryCacheContext>>();
    }

    /// <summary>
    /// Locates a cache context responsible for storing elements that share the same context fingerprint. If there is no matching context
    /// available then a new one is created and returned. This operation is performed under a lock covering the pool of available contexts
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="contextFingerPrint"></param>
    /// <param name="cacheDuration"></param>
    /// <returns></returns>
    public ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, string contextFingerPrint, TimeSpan cacheDuration)
    {
      lock (Contexts)
      {
        if (Contexts.TryGetValue(contextFingerPrint, out ITRexSpatialMemoryCacheContext context))
          return context; // It exists, return it

        // Create the new context
        ITRexSpatialMemoryCacheContext newContext = new TRexSpatialMemoryCacheContext(this, MRUList, cacheDuration, contextFingerPrint); 
        Contexts.Add(contextFingerPrint, newContext);

        lock (ProjectContexts)
        {
          // Add a reference to this context into the project specific list of contexts
          if (ProjectContexts.TryGetValue(projectUid, out var projectList))
            projectList.Add(newContext);
          else
            ProjectContexts.Add(projectUid, new List<ITRexSpatialMemoryCacheContext> {newContext});
        }

        return newContext;
      }
    }

    /// <summary>
    /// Locates a cache context responsible for storing elements that share the same context fingerprint. If there is no matching context
    /// available then a new one is created and returned. This operation is performed under a lock covering the pool of available contexts
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="contextFingerPrint"></param>
    /// <returns></returns>
    public ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, string contextFingerPrint)
    {
      return LocateOrCreateContext(projectUid, contextFingerPrint, TRexSpatialMemoryCacheContext.NullCacheTimeSpan);
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
      Interlocked.Increment(ref currentNumElements);

      // Increment the memory usage in the cache
      Interlocked.Add(ref currentSizeInBytes, sizeInBytes);
    }

    public void ItemRemovedFromContext(int sizeInBytes)
    {
      // Decrement the memory usage in the cache
      var number = Interlocked.Add(ref currentSizeInBytes, -sizeInBytes);

      if (number < 0)
      {
        Debug.Assert(false, "CurrentSizeInBytes < 0! Consider using Cache.Add(context, item).");
      }

      // Decrement the number of elements in the cache
      Interlocked.Decrement(ref currentNumElements);
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

    /// <summary>
    /// Invalidates subgrids held within all cache contexts for a project that are sensitive to
    /// ingest of production data (eg: from TAG files)
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="mask"></param>
    public void InvalidateDueToProductionDataIngest(Guid projectUid, ISubGridTreeBitMask mask)
    {
      List<ITRexSpatialMemoryCacheContext> projectContexts;

      // Obtain the list of contexts for this project
      lock (Contexts)
      {
        if (!ProjectContexts.TryGetValue(projectUid, out projectContexts))
        {
          // There are no cache contexts for this project, leave...
          return;
        }

        // Make a clone of this list to facilitate working through the evictions without holding
        // a long term lock on the global set of contexts
        projectContexts = new List<ITRexSpatialMemoryCacheContext>(projectContexts);
      }

      if (projectContexts.Count == 0)
        return;

      int numInvalidatedSubgrids = 0;
      int numScannedSubgrids = 0;
      DateTime startTime = DateTime.Now;

      // Walk through the cloned list of contexts evicting all relevant element per the supplied mask
      // Only hold a Contexts lock for the duration of a single context. 'Eviction' is really marking the 
      // element as dirty to amortize the effort in executing the invalidation across cache accessor contexts.
      foreach (var context in projectContexts)
      {
        lock (context)
        {
          // Empty contexts are ignored
          if (context.TokenCount == 0)
            continue;

          // If the context in question is not sensitive to production data ingest then ignore it
          if ((context.Sensitivity & TRexSpatialMemoryCacheInvalidationSensitivity.ProductionDataIngest) == 0)
            continue;

          // Iterate across all elements in the mask:
          // 1. Locate the cache entry
          // 2. Mark it as dirty
          mask.ScanAllSetBitsAsSubGridAddresses(origin =>
          {
            context.InvalidateSubgridNoLock(origin.X, origin.Y, out bool subGridPresentForInvalidation);

            numScannedSubgrids++;
            if (subGridPresentForInvalidation)
              numInvalidatedSubgrids++;
          });
        }
      }

      log.LogInformation($"Invalidated {numInvalidatedSubgrids} out of {numScannedSubgrids} scanned subgrid from {projectContexts.Count} contexts in {DateTime.Now - startTime} [project {projectUid}]");
    }
  }
}
