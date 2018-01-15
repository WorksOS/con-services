using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Base class for proxies getting master data from services.
  /// </summary>
  public class BaseProxy 
  {
    protected readonly ILogger log;
    private readonly ILoggerFactory logger;
    protected readonly IConfigurationStore configurationStore;
    private readonly IMemoryCache cache;

    protected BaseProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache)
    {
      log = logger.CreateLogger<BaseProxy>();
      this.logger = logger;
      this.configurationStore = configurationStore;
      this.cache = cache;
    }

    /// <summary>
    /// This constructor can be used when a proxy does not use caching
    /// </summary>
    /// <param name="configurationStore"></param>
    /// <param name="logger"></param>
    protected BaseProxy(IConfigurationStore configurationStore, ILoggerFactory logger)
    {
      log = logger.CreateLogger<BaseProxy>();
      this.logger = logger;
      this.configurationStore = configurationStore;
    }

    /// <summary>
    /// Executes a request against masterdata service
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <param name="method">Http method, defaults to POST</param>
    /// <returns>The item</returns>
    protected async Task<T> SendRequest<T>(string urlKey, string payload, IDictionary<string, string> customHeaders, string route = null, string method="POST")
    {
      var url = ExtractUrl(urlKey, route);
      T result = default(T);
      try
      {
        GracefulWebRequest request = new GracefulWebRequest(logger, configurationStore);
        result = await request.ExecuteRequest<T>(url, method, customHeaders, payload);
        log.LogDebug("Result of send to master data request: {0}", result);
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
        log.LogWarning("Error sending data from master data: ", message);
        log.LogWarning("Stacktrace: ", stacktrace);
        throw;
      }
      return result;
    }

    /// <summary>
    /// Gets a list of the specified items from the specified service. No caching.
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of items</returns>
    private async Task<List<T>> GetMasterDataList<T>(string urlKey, IDictionary<string, string> customHeaders, string route = null) 
    {
      var url = ExtractUrl(urlKey, route);

      List<T> result = null;
      try
      {
        GracefulWebRequest request = new GracefulWebRequest(logger, configurationStore);
        result = await request.ExecuteRequest<List<T>>(url, "GET", customHeaders);
        log.LogDebug($"Result of get list request: {JsonConvert.SerializeObject(result)}");
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
        throw;
      }
      return result;
    }

    /// <summary>
    /// Gets an item from the specified service. No caching.
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="queryParams">Query parameters for the request (optional)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of items</returns>
    protected async Task<T> GetMasterDataItem<T>(string urlKey, IDictionary<string, string> customHeaders, string queryParams=null, string route=null)
    {
      var url = ExtractUrl(urlKey, route);
      if (!string.IsNullOrEmpty(queryParams))
      {
        url += queryParams;
      }

      T result = default(T);
      try
      {
        GracefulWebRequest request = new GracefulWebRequest(logger, configurationStore);
        result = await request.ExecuteRequest<T>(url, "GET", customHeaders);
        log.LogDebug($"Result of get item request: {JsonConvert.SerializeObject(result)}");
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
        throw;
      }
      return result;
    }

    /// <summary>
    /// Gets a master data item. If the item is not in the cache then requests the item from the relevant service and adds it to the cache.
    /// </summary>
    /// <param name="uid">The UID of the item to retrieve. Also the cache key</param>
    /// <param name="cacheLifeKey">The configuration store key for how long to cache items</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>Master data item</returns>
    protected async Task<T> GetMasterDataItem<T>(string uid, string cacheLifeKey, string urlKey, IDictionary<string, string> customHeaders, string route = null) 
    {
      if (cache == null)
      {
        throw new InvalidOperationException("This method requires a cache; use the correct constructor");
      }
      ClearCacheIfRequired<T>(uid, customHeaders);
      var cacheKey = GetCacheKey<T>(uid);
      T cacheData;
      if (!cache.TryGetValue(cacheKey, out cacheData))
      {
        log.LogDebug($"Item for key {cacheKey} not found in cache, getting from web api");
        var opts = MemoryCacheExtensions.GetCacheOptions(cacheLifeKey, configurationStore, log);

        cacheData = await GetMasterDataItem<T>(urlKey, customHeaders, null, route);
        cache.Set(cacheKey, cacheData, opts);
        
      }
      else
      {
        log.LogDebug($"Found item for key {cacheKey} in cache");
      }
      return cacheData;
    }

    /// <summary>
    /// Gets a list of master data items for a customer. 
    /// If the list is not in the cache then requests items from the relevant service and adds the list to the cache.
    /// </summary>
    /// <param name="customerUid">The customer UID for the list to retrieve. Also the cache key.</param>
    /// <param name="cacheLifeKey">The configuration store key for how long to cache the list</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of Master data items</returns>
    protected async Task<List<T>> GetMasterDataList<T>(string customerUid, string cacheLifeKey, string urlKey,
                IDictionary<string, string> customHeaders, string route = null) 
    {
      if (cache == null)
      {
        throw new InvalidOperationException("This method requires a cache; use the correct constructor");
      }
      ClearCacheIfRequired<T>(customerUid, customHeaders);
      var cacheKey = GetCacheKey<T>(customerUid);
      List<T> cacheData;
      if (!cache.TryGetValue(cacheKey, out cacheData))
      {
        var opts = MemoryCacheExtensions.GetCacheOptions(cacheLifeKey, configurationStore, log);

        cacheData = await GetMasterDataList<T>(urlKey, customHeaders, route);
        cache.Set(cacheKey, cacheData, opts);
      }
      return cacheData;
    }

    /// <summary>
    /// Gets a list of master data items for a customer or project where the list is contained in (a property of) an object. 
    /// If the list is not in the cache then requests items from the relevant service and adds the list to the cache.
    /// </summary>
    /// <param name="uid">The UID for the list to retrieve (customerUid or projectUid). Also used for the cache key</param>
    /// <param name="cacheLifeKey">The configuration store key for how long to cache the list</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="queryParams">Query parameters for the request (optional)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of Master data items</returns>
    protected async Task<T> GetContainedMasterDataList<T>(string uid, string cacheLifeKey, string urlKey,
                  IDictionary<string, string> customHeaders, string queryParams = null, string route = null)
      {
        if (cache == null)
        {
          throw new InvalidOperationException("This method requires a cache; use the correct constructor");
        }
        ClearCacheIfRequired<T>(uid, customHeaders);
        var cacheKey = GetCacheKey<T>(uid);
        T cacheData;
        if (!cache.TryGetValue(cacheKey, out cacheData))
        {
          var opts = MemoryCacheExtensions.GetCacheOptions(cacheLifeKey, configurationStore, log);

        cacheData = await GetMasterDataItem<T>(urlKey, customHeaders, queryParams, route);
          cache.Set(cacheKey, cacheData, opts);
        }
        return cacheData;
      }
    /// <summary>
    /// Gets the requested base URL from the configuration and adds the route to get the full URL.
    /// </summary>
    /// <param name="urlKey">The configuration key for the URL to get</param>
    /// <param name="route">Any additional routing</param>
    /// <returns></returns>
    private string ExtractUrl(string urlKey, string route)
    {
      string url = configurationStore.GetValueString(urlKey);
      log.LogInformation(string.Format("{0}: {1}, route={2}", urlKey, url, route));

      if (string.IsNullOrEmpty(url))
      {
        var errorString = string.Format("Your application is missing an environment variable {0}", urlKey);
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
      if (!string.IsNullOrEmpty(route))
      {
        url += route;
      }
      return url;
    }

    /// <summary>
    /// Gets the cache key. MemoryCache is shared so we need to construct a unique cache key by uid and type of item. 
    /// </summary>
    /// <typeparam name="T">The type of item being cached</typeparam>
    /// <param name="uid">The uid of the item being cached</param>
    /// <returns>The cache key to use.</returns>
    private string GetCacheKey<T>(string uid)
    {
      var keyPrefix = typeof(T).Name;
      return $"{keyPrefix} {uid}";
    }

    /// <summary>
    /// Clears an item from the cache if requested in the headers.
    /// </summary>
    /// <typeparam name="T">The type of item being cached</typeparam>
    /// <param name="uid">The item to remove from the cache</param>
    /// <param name="customHeaders">The request headers</param>
    private void ClearCacheIfRequired<T>(string uid, IDictionary<string, string> customHeaders)
    {
      string caching = null;
      customHeaders.TryGetValue("X-VisionLink-ClearCache", out caching);
      if (!string.IsNullOrEmpty(caching) && caching == "true")
        ClearCacheItem<T>(uid);
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <typeparam name="T">The type of item being cached</typeparam>
    /// <param name="uid">The uid of the item to remove from the cache</param>
    protected void ClearCacheItem<T>(string uid)
    {
      if (cache == null)
      {
        throw new InvalidOperationException("This method requires a cache; use the correct constructor");
      }
      var cacheKey = GetCacheKey<T>(uid);
      log.LogDebug($"Clearing item from cache: {cacheKey}");
      cache.Remove(cacheKey);
    }

    /// <summary>
    /// Gets the key to use for caching when values are by project and user.
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <param name="customHeaders">HTTP request custom headers containing the user UID</param>
    /// <returns></returns>
    protected string CacheKeyByUser(string projectUid, IDictionary<string, string> customHeaders)
    {
      const string USER_UID_HEADER = "X-VisionLink-UserUid";
      if (!customHeaders.ContainsKey(USER_UID_HEADER))
      {
        throw new Exception($"Missing custom header {USER_UID_HEADER}");
      }
      return $"{projectUid} {customHeaders[USER_UID_HEADER]}";
    }
  }
}
