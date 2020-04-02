using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VSS.Common.Abstractions.Cache.Models;

namespace VSS.Common.Abstractions.Cache.Interfaces
{
  public interface IDataCache
  {
    /// <summary>
    /// List of all keys stored in the cache
    /// </summary>
    List<string> CacheKeys { get; }

    /// <summary>
    /// Identify tags at a given point in time
    /// Used in testing
    /// </summary>
    List<string> CacheTags { get; }

    /// <summary>
    /// Get a list of tags used to create the cache item for a given key
    /// </summary>
    List<string> GetTagsForKey(string key);

    /// <summary>
    /// Get an item from cache if it exists, and is of the correct type
    /// </summary>
    /// <typeparam name="TItem">Reference type expected to be cached</typeparam>
    /// <param name="key">Cache key for the object</param>
    /// <returns>null if the object doesn't exist in cache, else the cached object</returns>
    TItem Get<TItem>(string key) where TItem : class;

    /// <summary>
    /// Get an item from cache if it exists, or create it if the item is not cached
    /// </summary>
    /// <typeparam name="TItem">Reference type expected to be cached</typeparam>
    /// <param name="key">Cache key for the object</param>
    /// <param name="factory">The Function that will create the cached item, and tags if the item is not in cache</param>
    /// <returns>The item from cache, or from factory. Can return null if the factory returns null (No item will be cached)</returns>
    Task<TItem> GetOrCreate<TItem>(string key, Func<ICacheEntry, Task<CacheItem<TItem>>> factory)  where TItem : class;

    /// <summary>
    /// Set a cached item given a specified key, will create if it doesn't exist or override if it did exist
    /// </summary>
    /// <typeparam name="TItem">Reference type expected to be cached</typeparam>
    /// <param name="key">Cache key for the object</param>
    /// <param name="value">Value to cache</param>
    /// <param name="tags">Any tags that are associated with the cached item (used for cache invalidation)</param>
    /// <param name="options">Optional MemoryCacheOptions required</param>
    /// <returns>The value passed in.</returns>
    TItem Set<TItem>(string key, TItem value, IEnumerable<string> tags, MemoryCacheEntryOptions options = null)  where TItem : class;

    /// <summary>
    /// Remove any items in cache that are tagged with the specified tag
    /// </summary>
    /// <param name="tag">The tag to look for on any cached items</param>
    void RemoveByTag(string tag);

    /// <summary>
    /// Removed an item if it exists from cache
    /// </summary>
    /// <param name="key">The Cache key to remove</param>
    void RemoveByKey(string key);
  }
}
