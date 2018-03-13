using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;

namespace VSS.MasterData.Proxies
{
  public static class MemoryCacheExtensions
  {
    /// <summary>
    /// Get the cache options for the cache items. Sets the cache life.
    /// </summary>
    /// <param name="cacheLifeKey">The configuration key for the cache life</param>
    /// <returns>Memory cache options for the items</returns>
    public static MemoryCacheEntryOptions GetCacheOptions(this MemoryCacheEntryOptions opts, string cacheLifeKey, IConfigurationStore configurationStore,
      ILogger log)
    {
      const string DEFAULT_TIMESPAN_MESSAGE = "Using default 15 mins.";

      string cacheLife = configurationStore.GetValueString(cacheLifeKey);
      log.LogInformation($"{cacheLifeKey}: {cacheLife}");

      if (string.IsNullOrEmpty(cacheLife))
      {
        log.LogWarning(
          $"Your application is missing an environment variable {cacheLifeKey}. {DEFAULT_TIMESPAN_MESSAGE}");
        cacheLife = "00:15:00";
      }

      TimeSpan result;
      if (!TimeSpan.TryParse(cacheLife, out result))
      {
        log.LogWarning($"Invalid timespan for environment variable {cacheLifeKey}. {DEFAULT_TIMESPAN_MESSAGE}");
        result = new TimeSpan(0, 15, 0);
      }

      opts.SlidingExpiration = result;
      return opts;
    }

    static object cacheLock = new object();
    public static T GetOrAdd<T>(this IMemoryCache cache, string cacheKey, MemoryCacheEntryOptions opts, Func<T> factory)
    {
      if (cache.TryGetValue(cacheKey, out var promise))
        return ((Lazy<T>)promise).Value;

      Lazy<T> promiseToSet;

      lock (cacheLock)
      {
        if (cache.TryGetValue(cacheKey, out var promiseBeforeSet))
          return ((Lazy<T>)promiseBeforeSet).Value;
        promiseToSet = new Lazy<T>(factory);
        cache.Set(cacheKey, promiseToSet);
      }

      return promiseToSet.Value;
    }

    public static T Add<T>(this IMemoryCache cache, string cacheKey, MemoryCacheEntryOptions opts, Func<T> factory)
    {
      Lazy<T> promiseToSet;

      lock (cacheLock)
      {
        promiseToSet = new Lazy<T>(factory);
        cache.Set(cacheKey, promiseToSet);
      }

      return promiseToSet.Value;
    }
  }
}
