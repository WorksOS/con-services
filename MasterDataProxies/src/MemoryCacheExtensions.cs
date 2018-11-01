using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
      if (cache.TryGetValue(cacheKey, out var promise) && IsPromiseSuitable(promise as Lazy<T>))
        return ((Lazy<T>) promise).Value;

      Lazy<T> promiseToSet;

      lock (cacheLock)
      {
        if (cache.TryGetValue(cacheKey, out var promiseBeforeSet) && IsPromiseSuitable(promise as Lazy<T>))
            return ((Lazy<T>) promiseBeforeSet).Value;

        promiseToSet = new Lazy<T>(factory, LazyThreadSafetyMode.PublicationOnly);
        cache.Set(cacheKey, promiseToSet);
      }

      return promiseToSet.Value;
    }

    public static T Add<T>(this IMemoryCache cache, string cacheKey, MemoryCacheEntryOptions opts, Func<T> factory)
    {
      Lazy<T> promiseToSet;

      lock (cacheLock)
      {
        promiseToSet = new Lazy<T>(factory, LazyThreadSafetyMode.PublicationOnly);
        cache.Set(cacheKey, promiseToSet);
      }

      return promiseToSet.Value;
    }

    /// <summary>
    /// Checks a promise to see if it is suitable to be returned.
    /// We don't want promises that cause exceptions / invalid values to be returned as the issue may have been resolved.
    /// </summary>
    private static bool IsPromiseSuitable<T>(Lazy<T> promise)
    {
      if (promise == null)
        return false;

      var task = promise.Value as Task;
      // If we don't have a task for the promise, then the value created will only be true if the cached item didn't throw an exception
      if (task == null) 
        return promise.IsValueCreated;

      // Only return tasks that have ran to completion successfully (unhandled exceptions will cause a status of 'Faulted')
      return task.Status == TaskStatus.RanToCompletion;
    }
  }
}
