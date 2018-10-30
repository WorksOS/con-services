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
    /// Provides an estimation of the memory consumption of the element when stored in the cache
    /// </summary>
    /// <returns></returns>
    int IndicativeSizeInBytes();

    uint CacheOriginX { get; }
    uint CacheOriginY { get; }
  }
}
