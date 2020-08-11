using System;
using System.Threading;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Represents a context within the overall memory cache where all the elements within the context
  /// are related, such as being members of spatial data results returned from filters with the same fingerprint
  /// </summary>
  public class TRexSpatialMemoryCacheContext : ITRexSpatialMemoryCacheContext, IDisposable
  {
    /// <summary>
    /// The project for which this cache context stores items
    /// </summary>
    public Guid ProjectUID { get; }

    /// <summary>
    /// THe fingerprint used to distinguish this cache context from others stored in the overall cache
    /// </summary>
    public string FingerPrint { get; }

    /// <summary>
    /// The underlying grid data type this cache context is storing
    /// </summary>
    public GridDataType GridDataType { get; }

    /// <summary>
    /// Notes if this context has been marked for removal, for instance as a result of the last element within it
    /// being evicted or removed due to invalidation. Contexts marked for removal are in a zombie state that either ends
    /// in the concrete removal and destruction of the context, or the context is retrieved from the cache, or if an
    /// element is added to the context.
    /// </summary>
    public bool MarkedForRemoval { get; set; }

    public DateTime MarkedForRemovalAtUtc { get; set; } = Consts.MIN_DATETIME_AS_UTC;

    public ITRexSpatialMemoryCache OwnerMemoryCache { get; private set; }

    public IGenericSubGridTree_Int ContextTokens { get; private set; }

    public ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; private set; }

    private int _tokenCount;
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int TokenCount => _tokenCount;

    public TimeSpan CacheDurationTime { get; }

    /// <summary>
    /// The default cache expiry time period for new contexts
    /// </summary>
    public static readonly TimeSpan NullCacheTimeSpan = TimeSpan.FromDays(1000);

    /// <summary>
    /// Determine what external stimuli elements in this cache are sensitive to with respect to
    /// invalidation and eviction of elements contained in the cache.
    /// </summary>
    public TRexSpatialMemoryCacheInvalidationSensitivity Sensitivity { get; } = TRexSpatialMemoryCacheInvalidationSensitivity.ProductionDataIngest;

    /// <summary>
    /// Constructs a new cache context for the given owning cache and MRU list. Time base expiry is defaulted to 'never'.
    /// </summary>
    public TRexSpatialMemoryCacheContext(ITRexSpatialMemoryCache ownerMemoryCache,
      ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> mruList) : this(ownerMemoryCache, mruList, NullCacheTimeSpan, null, GridDataType.All, Guid.Empty)
    {
    }

    /// <summary>
    /// Constructs a new cache context for the given owning cache and MRU list. Time base expiry is controlled by
    /// specifying a cacheDurationTime each element will observe before proactive removal from the cache.
    /// </summary>
    public TRexSpatialMemoryCacheContext(ITRexSpatialMemoryCache ownerMemoryCache,
      ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> mruList,
      TimeSpan cacheDurationTime, string fingerPrint, GridDataType gridDataType, Guid projectUid)
    {
      ContextTokens = new GenericSubGridTree_Int(SubGridTreeConsts.SubGridTreeLevels - 1, 1);

      MRUList = mruList;
      CacheDurationTime = cacheDurationTime;
      FingerPrint = fingerPrint ?? Guid.NewGuid().ToString();
      GridDataType = gridDataType;
      ProjectUID = projectUid;
      OwnerMemoryCache = ownerMemoryCache;
    }

    /// <summary>
    /// Adds an item into a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    public CacheContextAdditionResult Add(ITRexMemoryCacheItem element)
    {
      var x = element.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      var y = element.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      // Add the element to storage and obtain its index in that storage, inserting it into the context
      // Note: The index is added as a 1-based index to the ContextTokens to differentiate it from the null value
      // of 0 used as the null value in integer based sub grid trees
      if (ContextTokens[x, y] != 0)
      {
        // This cache element in the context already contains an item.
        // Do not overwrite the present element with the one provided
        return CacheContextAdditionResult.AlreadyExisting;
      }

      var MRUIndex = MRUList.Add(element, this);
      if (MRUIndex == -1)
        return CacheContextAdditionResult.MRUListFull;

      ContextTokens[x, y] = MRUIndex + 1;
      Interlocked.Increment(ref _tokenCount);

      return CacheContextAdditionResult.Added;
    }

    /// <summary>
    /// Removes an item from a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    public void Remove(ITRexMemoryCacheItem element)
    {
      var x = element.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      var y = element.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      MRUList.Remove(ContextTokens[x, y] - 1);
      ContextTokens[x, y] = 0;

      // If the context has been emptied by the removal of this item then mark as a candidate for removal
      if (Interlocked.Decrement(ref _tokenCount) == 0)
      {
        MarkForRemoval(DateTime.UtcNow);
      }
    }

    /// <summary>
    /// Gets an item held in the cache described by the origin cell coordinate fo the sub grid
    /// </summary>
    public ITRexMemoryCacheItem Get(int originX, int originY)
    {
      var x = originX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      var y = originY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      var index = ContextTokens[x, y];

      if (index == 0)
      {
        return null;
      }
      else
      {
        return MRUList.Get(index - 1);
      }
    }

    /// <summary>
    /// Removes the index for an item from the context token sub grid tree only. This is intended to be used by the MRU list to communicate
    /// elements that are being removed from the MRUList in response to adding new items to the cache.
    /// Note: This operations executes within a WriteLock obtained from the owning SubGridTree in an ancestor calling context
    /// </summary>
    public void RemoveFromContextTokensOnly(ITRexMemoryCacheItem item)
    {
      var x = item.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      var y = item.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      ContextTokens[x, y] = 0;
      Interlocked.Decrement(ref _tokenCount);

      OwnerMemoryCache.ItemRemovedFromContext(item.IndicativeSizeInBytes());
    }

    /// <summary>
    /// Invalidates the cached item within this context at the specified origin location
    /// </summary>
    public void InvalidateSubGrid(int originX, int originY, out bool subGridPresentForInvalidation)
    {
      subGridPresentForInvalidation = false;

      var x = originX >> SubGridTreeConsts.SubGridIndexBitsPerLevel;
      var y = originY >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

      var contextToken = ContextTokens[x, y];

      if (contextToken != 0)
      {
        // Note: the index in the ContextTokens tree is 1-based, so account for that in the call to Invalidate
        var item = MRUList.Get(contextToken - 1);
        Remove(item);

        subGridPresentForInvalidation = true;
      }
    }

    /// <summary>
    /// Invalidates all sub grids for context
    /// </summary>
    public void InvalidateAllSubGrids()
    {
      // Empty contexts are ignored
      if (_tokenCount == 0)
        return;

      ContextTokens.ScanAllSubGrids(leaf =>
      {
        SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var contextToken = ((IGenericLeafSubGrid<int>)leaf).Items[x, y];
          if (contextToken != 0)
          {
            // Note: the index in the ContextTokens tree is 1-based, so account for that in the call to Invalidate
            var item = MRUList.Get(contextToken - 1);
            Remove(item);
          }
        });
        return true;
      });
    }

    /// <summary>
    /// Mark this context as a candidate for removal
    /// </summary>
    public void MarkForRemoval(DateTime markedForRemovalAtUtc)
    {
      if (markedForRemovalAtUtc.Kind != DateTimeKind.Utc)
        throw new TRexException("MarkForRemoval is not a UTC date");

      MarkedForRemovalAtUtc = markedForRemovalAtUtc;
      MarkedForRemoval = true;
    }

    /// <summary>
    /// Reanimates the context from being marked for removal to actively used
    /// </summary>
    public void Reanimate()
    {
      MarkedForRemovalAtUtc = Consts.MIN_DATETIME_AS_UTC;
      MarkedForRemoval = false;
    }

    public void Dispose()
    {
      ContextTokens?.Dispose();
      ContextTokens = null;
      OwnerMemoryCache = null;
      MRUList = null;
    }
  }
}
