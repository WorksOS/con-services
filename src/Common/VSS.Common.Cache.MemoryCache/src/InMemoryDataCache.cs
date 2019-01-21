using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;

namespace VSS.Common.Cache.MemoryCache
{
  public class InMemoryDataCache : IDataCache
  {
    private readonly ILogger<InMemoryDataCache> logger;
    private readonly IMemoryCache cache;

    /// <summary>
    /// To save time in this code, we won't bother building a string to log if isn't going to be logged
    /// </summary>
    private bool isLogEnabled = false;

    /// <summary>
    /// Store a dictionary of Cache Keys to Tags associated to the cached item
    /// </summary>
    private readonly ConcurrentDictionary<string, List<string>> keyLookup = new ConcurrentDictionary<string, List<string>>();

    public InMemoryDataCache(ILoggerFactory loggerFactory, IMemoryCache cache)
    {
      logger = loggerFactory.CreateLogger<InMemoryDataCache>();
      isLogEnabled = logger.IsEnabled(LogLevel.Trace);

      this.cache = cache;
    }

    /// <summary>
    /// A list of keys cached currently
    /// Note: this is a snapshot in time
    /// </summary>
    public List<string> CacheKeys => keyLookup.Keys.ToList();

    /// <summary>
    /// Not meant to be used to identify tags, but handy for a instant in time overview
    /// Used in testing
    /// </summary>
    public List<string> CacheTags => keyLookup.Values.SelectMany(s => s).Distinct().ToList();

    /// <summary>
    /// Get an item from cache if it exists, and is of the correct type
    /// </summary>
    /// <typeparam name="TItem">Reference type expected to be cached</typeparam>
    /// <param name="key">cache key for the object</param>
    /// <exception cref="InvalidCastException">The item cached is not of the expected type</exception>
    /// <returns>null if the object doesn't exist in cache, else the cached object</returns>
    public TItem Get<TItem>(string key) where TItem : class
    {
      if (cache.TryGetValue(key.ToLower(), out var obj))
      {
        if(isLogEnabled)
          logger.LogTrace($"Getting key {key} from cache has a hit, returning cached item.");
        return (TItem) obj;
      }

      if(isLogEnabled)
        logger.LogTrace($"Getting key {key} from cache has a miss, returning null");

      return null;
    }

    /// <summary>
    /// Get an item from cache if it exists, or create it if the item is not cached
    /// </summary>
    /// <typeparam name="TItem">Reference type expected to be cached</typeparam>
    /// <param name="key">cache key for the object</param>
    /// <param name="factory">The Function that will create the cached item, and tags if the item is not in cache</param>
    /// <exception cref="InvalidCastException">The item cached is not of the expected type</exception>
    /// <returns>The item from cache, or from factory. Can return null if the factory returns null (No item will be cached)</returns>
    public async Task<TItem> GetOrCreate<TItem>(string key, Func<ICacheEntry, Task<CacheItem<TItem>>> factory) where TItem : class
    {
      var lowerKey = key.ToLower();

      // IF we have our item in cache, returned it
      if (cache.TryGetValue(lowerKey, out var obj) && obj != null)
      {
        if(isLogEnabled)
          logger.LogTrace($"Fetching key {key} from cache has a hit, returned cached item");
        return (TItem) obj;
      }

      // We don't have a cached item, create one associated with our MemoryCache
      using (var entry = cache.CreateEntry(lowerKey))
      {
        // We want to be called back if the item is ejected from cache to update out internal state
        entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration()
        {
          EvictionCallback = EvictionCallback
        });

        // Call the factory func passed in by the calling code, this should populate
        var item = await factory(entry);

        if(isLogEnabled)
          logger.LogTrace($"Fetching key {key} from cache has a miss, creating a new item. Valid cache item: {item?.Value != null}");

        if (item?.Value == null)
          return null;

        // When we set the value in the Memory Cache, it copies it - so we can safely dispose it
        entry.SetValue(item.Value);
        AddKeyTagsToInternalState(lowerKey, item.Tags);

        return item.Value;
      }
    }

    /// <summary>
    /// Set a cached item given a specified key, will create if it doesn't exist or override if it did exist
    /// </summary>
    /// <typeparam name="TItem">Reference type expected to be cached</typeparam>
    /// <param name="key">Cache key for the object</param>
    /// <param name="value">Value to cache</param>
    /// <param name="tags">Any tags that are associated with the cached item (used for cache invalidation)</param>
    /// <param name="options">Optional MemoryCacheOptions required</param>
    /// <returns>The value passed in.</returns>
    public TItem Set<TItem>(string key, TItem value, IEnumerable<string> tags, MemoryCacheEntryOptions options = null) where TItem : class
    {
      if(isLogEnabled)
        logger.LogTrace($"Setting new cache item with key {key}");

      var lowerKey = key.ToLower();

      if (options == null)
        options = new MemoryCacheEntryOptions();

      options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration()
      {
        EvictionCallback  = EvictionCallback
      });

      var t = tags?.ToList();

      cache.Set(lowerKey, value, options);

      AddKeyTagsToInternalState(lowerKey, t);
      
      return value;
    }

    /// <summary>
    /// Remove any items in cache that are tagged with the specified tag
    /// </summary>
    /// <param name="tag">The tag to look for on any cached items</param>
    public void RemoveByTag(string tag)
    {
      if(isLogEnabled)
        logger.LogTrace($"Removing Cache items via the tag {tag}");

      // ToList will create a snapshot of the dictionary as at call time
      // If anything is changed after, so be it 
      // Remove By Key will only attempt to remove it if it exists
      // https://stackoverflow.com/a/14636272/18405
      var keys = keyLookup.ToList()
        .Where(k => k.Value.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .Select(k => k.Key)
        .ToList();

      for(var i = 0; i < keys.Count; i++)
        RemoveByKey(keys[i]);
    }

    /// <summary>
    /// Removed an item if it exists from cache
    /// </summary>
    /// <param name="key">The Cache key to remove</param>
    public void RemoveByKey(string key)
    {
      var lowerKey = key.ToLower();
      if(isLogEnabled)
        logger.LogTrace($"Removing Cache items via the key {key}");
      // We need to remove from our internal state, which doesn't directly update the cache
      RemoveKeyFromInternalState(lowerKey);

      // So we remove from the cache
      cache.Remove(lowerKey);
    }

    /// <summary>
    /// Removes the key from our internal state, but not the actual cache
    /// </summary>
    private void RemoveKeyFromInternalState(string key)
    {
      // Have this key got any tags stored?
      if (!keyLookup.TryRemove(key, out _))
      {
        if(isLogEnabled)
          logger.LogTrace($"Failed to remove {key} from internal state, it may not exist anymore");
      }
    }

    /// <summary>
    /// This call back is called from the MemoryCache when an item is evicted from cache
    /// Note: This won't happen when the cache timeout happens,
    /// But rather when the cached item is attempted to be fetched after timeout (but before a new one is created)
    /// </summary>
    private void EvictionCallback(object key, object value, EvictionReason reason, object state)
    {
      if (reason == EvictionReason.Replaced) return;
      if (key is string k)
      {
        if(isLogEnabled)
          logger.LogTrace($"Key {k} was evicted from cache. Reason: {reason}");
        RemoveKeyFromInternalState(k);
      }
    }

    /// <summary>
    /// Add the tags for any key (if tags is empty it removes the key from our internal state)
    /// </summary>
    private void AddKeyTagsToInternalState(string key, List<string> tags)
    {
      if (tags == null || tags.Count == 0)
        RemoveKeyFromInternalState(key); // This does not removed the item from cache, just from our internal state
      else
        keyLookup[key] = tags;
    }
  }
}
