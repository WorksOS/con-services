using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Proxies.Models;


namespace VSS.Raptor.Service.Common.Proxies
{
  /// <summary>
  /// Base class for proxies getting master data from services.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class BaseProxy<T> where T : IData
  {
    protected readonly ILogger log;
    private readonly ILoggerFactory logger;
    private IConfigurationStore configurationStore;
    private IMemoryCache cache;


    public BaseProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache)
    {
      log = logger.CreateLogger<BaseProxy<T>>();
      this.logger = logger;
      this.configurationStore = configurationStore;
      this.cache = cache;
    }

    /// <summary>
    /// Gets a list of the specified items from the specified service.
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUId and customerUId)</param>
    /// <returns></returns>
    protected List<T> GetList(string urlKey, IDictionary<string, string> customHeaders)
    {
      string url = configurationStore.GetValueString(urlKey);
      log.LogInformation(string.Format("{0}: {1}", urlKey, url));

      if (url == null)
      {
        var errorString = string.Format("Your application is missing an environment variable {0}", urlKey);
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }

      List<T> result = null;
      try
      {
        GracefulWebRequest request = new GracefulWebRequest(logger);
        result = request.ExecuteRequest<List<T>>(url, "GET", customHeaders).Result;
        log.LogDebug("Result of get master data list request: {0} items", result.Count);
      }
      catch (Exception ex)
      {
        string message = ex.Message;
        string stacktrace = ex.StackTrace;
        //Check for 400 and 500 errors which come through as an inner exception
        if (ex.InnerException != null)
        {
          message = ex.InnerException.Message;
          stacktrace = ex.InnerException.StackTrace;
        }
        log.LogWarning("Error getting data from master data: ", message);
        log.LogWarning("Stacktrace: ", stacktrace);
      }
      return result;
    }
    /// <summary>
    /// Gets a master data item. If the item is not in the cache then requests items from the relevant service and adds them to the cache.
    /// </summary>
    /// <param name="uid">The UID of the item to retrieve</param>
    /// <param name="cacheLife">How long to cache items</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUId and customerUId)</param>
    /// <returns></returns>
    protected T GetItem(string uid, TimeSpan cacheLife, string urlKey, IDictionary<string, string> customHeaders)
    {
      T cacheData;
      if (!cache.TryGetValue(uid, out cacheData))
      {
        var opts = new MemoryCacheEntryOptions()
        {
          SlidingExpiration = cacheLife
        };

        var list = GetList(urlKey, customHeaders);
        foreach (var item in list)
        {
          var data = item as IData;
          cache.Set(data.CacheKey, item, opts);
        }
        cache.TryGetValue(uid, out cacheData);
      }
      return cacheData;
    }
  }
}
