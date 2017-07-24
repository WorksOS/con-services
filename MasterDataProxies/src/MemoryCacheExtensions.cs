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
    public static MemoryCacheEntryOptions GetCacheOptions(string cacheLifeKey, IConfigurationStore configurationStore, ILogger log)
    {
      const string DEFAULT_TIMESPAN_MESSAGE = "Using default 15 mins.";

      string cacheLife = configurationStore.GetValueString(cacheLifeKey);
      log.LogInformation($"{cacheLifeKey}: {cacheLife}");

      if (string.IsNullOrEmpty(cacheLife))
      {
        log.LogWarning($"Your application is missing an environment variable {cacheLifeKey}. {DEFAULT_TIMESPAN_MESSAGE}");
        cacheLife = "00:15:00";
      }

      TimeSpan result;
      if (!TimeSpan.TryParse(cacheLife, out result))
      {
        log.LogWarning($"Invalid timespan for environment variable {cacheLifeKey}. {DEFAULT_TIMESPAN_MESSAGE}");
        result = new TimeSpan(0, 15, 0);
      }

      return new MemoryCacheEntryOptions()
      {
        SlidingExpiration = result
      };
    }
  }
}
