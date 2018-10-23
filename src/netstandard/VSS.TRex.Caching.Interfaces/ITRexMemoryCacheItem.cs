namespace VSS.TRex.Caching
{
  /// <summary>
  /// Defines the responsibilities of any element wishing to be referenced by the TRex MRU memory cache. TRex caching is implemented via a
  /// cooperative contract between the context owning cache elements, the cache elements themselves and the TRex memory cache implementation itself.
  /// Fortunately all these contexts are represented within the overall TRexSpatialMemoryCache implementation
  /// </summary>
  public interface ITRexMemoryCacheItem
  {
    /// <summary>
    /// The token assigned to the element in the memory cache at the time is it added to the cache, or modifying due to MRU adjustment
    /// This token is delegated to the element being stored in the cache and should be covered by concurrency control in the context
    /// of the element being stored in the cache. This removes the need to use global cache locks to manage concurrency at the fine grained
    /// cache element level.
    /// </summary>
    long MemoryCacheToken { get; set; }

    /// <summary>
    /// Provides an estimation of the memory consumption of the element when stored in the cache
    /// </summary>
    /// <returns></returns>
    int IndicativeSizeInBytes();

    uint OriginX { get; }
    uint OriginY { get; }
    int Level { get; }
  }
}
