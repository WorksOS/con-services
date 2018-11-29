using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  ///   Base class for proxies getting master data from services.
  /// </summary>
  public class BaseProxy
  {
    private readonly IMemoryCache cache;
    private readonly static AsyncDuplicateLock memCacheLock = new AsyncDuplicateLock();
    protected readonly IConfigurationStore configurationStore;
    protected readonly ILogger log;
    protected readonly ILoggerFactory logger;

    protected BaseProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache)
    {
      log = logger.CreateLogger<BaseProxy>();
      this.logger = logger;
      this.configurationStore = configurationStore;
      this.cache = cache;
    }

    /// <summary>
    ///   This constructor can be used when a proxy does not use caching
    /// </summary>
    /// <param name="configurationStore"></param>
    /// <param name="logger"></param>
    protected BaseProxy(IConfigurationStore configurationStore, ILoggerFactory logger)
    {
      log = logger.CreateLogger<BaseProxy>();
      this.logger = logger;
      this.configurationStore = configurationStore;
    }

    private async Task<T> SendRequestInternal<T>(string url, IDictionary<string, string> customHeaders,
      string method = "POST", string payload = null, Stream streamPayload = null)
    {
      var result = default(T);
      log.LogDebug($"Preparing {url} ({method}) headers {customHeaders.LogHeaders()}");
      try
      {
        var request = new GracefulWebRequest(logger, configurationStore);
        if (method != "GET")
        {
          if (streamPayload != null && payload == null)
            result = await request.ExecuteRequest<T>(url, streamPayload, customHeaders, method);
          else
          {
            if (payload != null)
            {
              streamPayload = new MemoryStream(Encoding.UTF8.GetBytes(payload));
              result = await request.ExecuteRequest<T>(url, streamPayload, customHeaders, method);
            }
          }
        }
        else
        {
          result = await request.ExecuteRequest<T>(url, method: "GET",customHeaders: customHeaders);
        }

        log.LogDebug("Result of send to master data request: {0}", result);
      }
      catch (Exception ex)
      {
        LogWebRequestException(ex);
        throw;
      }

      return result;
    }

    /// <summary>
    ///   Executes a request against masterdata service
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <param name="method">Http method, defaults to POST</param>
    /// <param name="queryParameters">Query parameters (optional)</param>
    /// <returns>The item</returns>
    protected async Task<T> SendRequest<T>(string urlKey, string payload, IDictionary<string, string> customHeaders,
      string route = null, string method = "POST", string queryParameters = null)
    {
      log.LogDebug($"Executing {urlKey} ({method}) {route} {queryParameters} {payload} {customHeaders.LogHeaders()}");
      return await SendRequestInternal<T>(ExtractUrl(urlKey, route, queryParameters), customHeaders, method, payload);
    }

    /// <summary>
    ///   Executes a request against masterdata service
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <param name="method">Http method, defaults to POST</param>
    /// <param name="queryParameters">Query parameters (optional)</param>
    /// <returns>The item</returns>
    protected async Task<T> SendRequest<T>(string urlKey, string payload, IDictionary<string, string> customHeaders,
      string route = null, string method = "POST", IDictionary<string, string> queryParameters = null)
    {
      log.LogDebug($"Executing {urlKey} ({method}) {route} {queryParameters} {payload} {customHeaders.LogHeaders()}");
      return await SendRequestInternal<T>(ExtractUrl(urlKey, route, queryParameters), customHeaders, method, payload);
    }

    /// <summary>
    ///   Executes a request against masterdata service
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <param name="method">Http method, defaults to POST</param>
    /// <returns>The item</returns>
    protected async Task<T> SendRequest<T>(string urlKey, Stream payload, IDictionary<string, string> customHeaders,
      string route = null, string method = "POST", IDictionary<string, string> queryParameters = null)
    {
      return await SendRequestInternal<T>(ExtractUrl(urlKey, route, queryParameters), customHeaders, method,
        streamPayload: payload);
    }


    /// <summary>
    ///   Executes a request against masterdata service
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <param name="queryParams"></param>
    /// <param name="method">Http method, defaults to POST</param>
    /// <returns>The item</returns>
    protected async Task<T> SendRequest<T>(string urlKey, Stream payload, IDictionary<string, string> customHeaders,
      string route = null, IDictionary<string, string> queryParams = null, string method = "POST")
    {
      return await SendRequestInternal<T>(ExtractUrl(urlKey, route, string.Empty), customHeaders, method,
        streamPayload: payload);
    }


    private async Task<K> GetObjectsFromMasterdata<K>(string urlKey, IDictionary<string, string> customHeaders,
      string queryParams = null, string route = null)
    {
      K result;

      var url = ExtractUrl(urlKey, route, queryParams);
      try
      {
        var request = new GracefulWebRequest(logger, configurationStore);
        result = await request.ExecuteRequest<K>(url, customHeaders: customHeaders, method: "GET");
        log.LogDebug($"Result of get item request: {JsonConvert.SerializeObject(result)}");
      }
      catch (Exception ex)
      {
        LogWebRequestException(ex);
        throw;
      }

      return result;
    }

    /// <summary>
    ///   Gets a list of the specified items from the specified service. No caching.
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of items</returns>
    private async Task<List<T>> GetMasterDataList<T>(string urlKey, IDictionary<string, string> customHeaders,
      string route = null)
    {
      return await GetObjectsFromMasterdata<List<T>>(urlKey, customHeaders, string.Empty, route);
    }

    /// <summary>
    ///   Gets an item from the specified service. No caching.
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="queryParams">Query parameters for the request (optional)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of items</returns>
    protected async Task<T> GetMasterDataItem<T>(string urlKey, IDictionary<string, string> customHeaders,
      string queryParams = null, string route = null)
    {
      return await GetObjectsFromMasterdata<T>(urlKey, customHeaders, queryParams, route);
    }

    /// <summary>
    ///   Gets an item from the specified service as Stream Content. No Caching
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="method">Http method, defaults to GET</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="queryParams">Query parameters for the request (optional)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of items</returns>
    protected async Task<Stream> GetMasterDataStreamContent(string urlKey,
      IDictionary<string, string> customHeaders, string method = "GET", string payload = null,
      string queryParams = null, string route = null)
    {
      Stream result = null;
      var url = ExtractUrl(urlKey, route, queryParams);
      try
      {
        var request = new GracefulWebRequest(logger, configurationStore);
        if (method != "GET")
        {
          if (payload != null)
          {
            var streamPayload = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            result = await (await request.ExecuteRequestAsStreamContent(url, method, customHeaders, streamPayload)).ReadAsStreamAsync();
          }
        }
        else
          result = await (await request.ExecuteRequestAsStreamContent(url, method, customHeaders)).ReadAsStreamAsync();
      }
      catch (Exception ex)
      {
        LogWebRequestException(ex);
        throw;
      }

      return result;
    }

    /// <summary>
    ///   Gets a master data item. If the item is not in the cache then requests the item from the relevant service and adds it
    ///   to the cache.
    /// </summary>
    /// <param name="uid">The UID of the item to retrieve. Also the cache key</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    /// <param name="cacheLifeKey">The configuration store key for how long to cache items</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>Master data item</returns>
    protected async Task<T> GetMasterDataItem<T>(string uid, string userId, string cacheLifeKey, string urlKey,
      IDictionary<string, string> customHeaders, string route = null)
    {
      return await WithMemoryCacheExecute(uid, userId, cacheLifeKey, customHeaders,
        () => GetMasterDataItem<T>(urlKey, customHeaders, null, route));
    }


    /// <summary>
    ///   Execute statement with MemoryCache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uid">The uid.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cacheLifeKey">The cache life key.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="action">The action.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///   This method requires a cache; use the correct constructor
    ///   or
    ///   Incorrect expiration time parameter
    /// </exception>
    private async Task<T> WithMemoryCacheExecute<T>(string uid, string userId, object cacheLifeKey,
      IDictionary<string, string> customHeaders, Func<Task<T>> action)
    {
      if (cache == null)
        throw new InvalidOperationException("This method requires a cache; use the correct constructor");
      var cacheKey = GetCacheKey<T>(uid, userId);
      var opts = new MemoryCacheEntryOptions();

      switch (cacheLifeKey)
      {
        case string s:
          opts.GetCacheOptions(s, configurationStore, log);
          break;
        case TimeSpan t:
          opts.SlidingExpiration = t;
          break;
        default:
          throw new InvalidOperationException("Incorrect expiration time parameter");
      }

      T result =default(T);

      using (await memCacheLock.LockAsync(cacheKey))
      {
        if (!IfCacheNeedsToBeInvalidated(customHeaders))
          return await cache.GetOrCreate(cacheKey, async entry =>
          {
            entry.SetOptions(opts);
            log.LogDebug($"Item for key {cacheKey} not found in cache, getting from web api");
            result = await action.Invoke();
            if (result != null) return result;
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                "Unable to request data from a webapi"));
          });

        log.LogDebug($"Item for key {cacheKey} is requested to be invalidated, getting from web api");
        result = await action.Invoke();
        if (result != null)
          return cache.Set(cacheKey, result, opts);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Unable to request data from a webapi"));
      }
    }


    /// <summary>
    ///   Gets a master data item. If the item is not in the cache then requests the item from the relevant service and adds it
    ///   to the cache.
    /// </summary>
    /// <param name="uid">The UID of the item to retrieve. Also the cache key</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    /// <param name="cacheLifeKey">The configuration store key for how long to cache items</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>Master data item</returns>
    protected async Task<T> GetMasterDataItem<T>(string uid, string userId, TimeSpan cacheLifeKey, string urlKey,
      IDictionary<string, string> customHeaders, string route = null)
    {
      return await WithMemoryCacheExecute(uid, userId, cacheLifeKey, customHeaders,
        () => GetMasterDataItem<T>(urlKey, customHeaders, null, route));
    }


    /// <summary>
    ///   Gets a list of master data items for a customer.
    ///   If the list is not in the cache then requests items from the relevant service and adds the list to the cache.
    /// </summary>
    /// <param name="customerUid">The customer UID for the list to retrieve. Also the cache key.</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    /// <param name="cacheLifeKey">The configuration store key for how long to cache the list</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of Master data items</returns>
    protected async Task<List<T>> GetMasterDataList<T>(string customerUid, string userId, string cacheLifeKey,
      string urlKey,
      IDictionary<string, string> customHeaders, string route = null)
    {
      return await WithMemoryCacheExecute(customerUid, userId, cacheLifeKey, customHeaders,
        () => GetMasterDataList<T>(urlKey, customHeaders, route));
    }

    /// <summary>
    ///   Gets a list of master data items for a customer or project where the list is contained in (a property of) an object.
    ///   If the list is not in the cache then requests items from the relevant service and adds the list to the cache.
    /// </summary>
    /// <param name="uid">The UID for the list to retrieve (customerUid or projectUid). Also used for the cache key</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    /// <param name="cacheLifeKey">The configuration store key for how long to cache the list</param>
    /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="queryParams">Query parameters for the request (optional)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of Master data items</returns>
    protected async Task<T> GetContainedMasterDataList<T>(string uid, string userId, string cacheLifeKey, string urlKey,
      IDictionary<string, string> customHeaders, string queryParams = null, string route = null)
    {
      return await WithMemoryCacheExecute(uid, userId, cacheLifeKey, customHeaders,
        () => GetMasterDataItem<T>(urlKey, customHeaders, queryParams, route));
    }

    /// <summary>
    ///   Gets the requested base URL from the configuration and adds the route to get the full URL.
    ///   Also adds any query parameters.
    /// </summary>
    /// <param name="urlKey">The configuration key for the URL to get</param>
    /// <param name="route">Any additional routing</param>
    /// <param name="queryParameters">Any query parameters</param>
    /// <returns></returns>
    protected string ExtractUrl(string urlKey, string route, string queryParameters = null)
    {
      var url = ExtractBaseUrl(urlKey, route);
      if (!string.IsNullOrEmpty(queryParameters))
        url += queryParameters;
      return url;
    }

    /// <summary>
    ///   Gets the requested base URL from the configuration and adds the route to get the full URL.
    ///   Also adds any query parameters.
    /// </summary>
    /// <param name="urlKey">The configuration key for the URL to get</param>
    /// <param name="route">Any additional routing</param>
    /// <param name="queryParameters">Any query parameters</param>
    /// <returns></returns>
    private string ExtractUrl(string urlKey, string route, IDictionary<string, string> queryParameters = null)
    {
      var url = ExtractBaseUrl(urlKey, route);
      if (queryParameters != null)
      {
        url += "?";
        url += new FormUrlEncodedContent(queryParameters)
          .ReadAsStringAsync().Result;
      }

      return url;
    }

    private string ExtractBaseUrl(string urlKey, string route)
    {
      var url = configurationStore.GetValueString(urlKey);
      log.LogInformation(string.Format("{0}: {1}, route={2}", urlKey, url, route));

      if (string.IsNullOrEmpty(url))
      {
        var errorString = string.Format("Your application is missing an environment variable {0}", urlKey);
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }

      if (!string.IsNullOrEmpty(route))
        url += route;
      return url;
    }

    /// <summary>
    ///   Gets the cache key. MemoryCache is shared so we need to construct a unique cache key by uid and type of item.
    /// </summary>
    /// <typeparam name="T">The type of item being cached</typeparam>
    /// <param name="uid">The uid of the item being cached</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    /// <returns>The cache key to use.</returns>
    private string GetCacheKey<T>(string uid, string userId)
    {
      var keyPrefix = typeof(T).Name;
      return string.IsNullOrEmpty(userId) ? $"{keyPrefix} {uid}" : $"{keyPrefix} {uid} {userId}";
    }

    /// <summary>
    ///   Clears an item from the cache if requested in the headers.
    /// </summary>
    /// <typeparam name="T">The type of item being cached</typeparam>
    /// <param name="uid">The item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    /// <param name="customHeaders">The request headers</param>
    private void ClearCacheIfRequired<T>(string uid, string userId, IDictionary<string, string> customHeaders)
    {
      if (IfCacheNeedsToBeInvalidated(customHeaders))
        ClearCacheItem<T>(uid, userId);
    }

    /// <summary>
    ///   Determines if the cache needs to be invalidated.
    /// </summary>
    /// <param name="customHeaders">The custom headers.</param>
    /// <returns></returns>
    private bool IfCacheNeedsToBeInvalidated(IDictionary<string, string> customHeaders)
    {
      customHeaders.TryGetValue("X-VisionLink-ClearCache", out var caching);
      return !string.IsNullOrEmpty(caching) && caching == "true";
    }

    /// <summary>
    ///   Clears an item from the cache
    /// </summary>
    /// <typeparam name="T">The type of item being cached</typeparam>
    /// <param name="uid">The uid of the item to remove from the cache</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    protected void ClearCacheItem<T>(string uid, string userId)
    {
      if (cache == null)
        throw new InvalidOperationException("This method requires a cache; use the correct constructor");
      var cacheKey = GetCacheKey<T>(uid, userId);
      log.LogDebug($"Clearing item from cache: {cacheKey}");
      cache.Remove(cacheKey);
    }

    /// <summary>
    ///   Gets an item from a list.
    /// </summary>
    /// <typeparam name="U">The type of item in the list</typeparam>
    /// <param name="listUid">The uid for the get request, also the cache key</param>
    /// <param name="getList">The methiod to call to get the list of items</param>
    /// <param name="itemSelector">The predicate to select the required item from the list</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <returns>The item</returns>
    private async Task<U> GetItemFromList<U>(string listUid,
      Func<string, IDictionary<string, string>, Task<List<U>>> getList,
      Func<U, bool> itemSelector,
      IDictionary<string, string> customHeaders = null)
    {
      var list = await getList(listUid, customHeaders);
      return list.SingleOrDefault(itemSelector);
    }

    /// <summary>
    ///   Check exception for Web Request details and log a warning
    /// </summary>
    /// <param name="ex">Exception to be logged</param>
    private void LogWebRequestException(Exception ex)
    {
      var message = ex.Message;
      var stacktrace = ex.StackTrace;
      //Check for 400 and 500 errors which come through as an inner exception
      if (ex.InnerException != null)
      {
        message = ex.InnerException.Message;
        stacktrace = ex.InnerException.StackTrace;
      }

      log.LogWarning("Error sending data from master data: ", message);
      log.LogWarning("Stacktrace: ", stacktrace);
    }

    /// <summary>
    ///   Gets an item from a list. If the item is not in the list then clears the cache and does the get again
    ///   which will issue the http request to get the list again. This lets us pick up items which have been
    ///   added since the list was cached (default 15 mins).
    /// </summary>
    /// <typeparam name="T">The type of the result from the http request</typeparam>
    /// <typeparam name="U">The type of item in the list</typeparam>
    /// <param name="getList">The method to call to get the list of items.</param>
    /// <param name="itemSelector">The predicate to select the required item from the list</param>
    /// <param name="listUid">The uid for the get request, also the cache key</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <returns></returns>
    public async Task<U> GetItemWithRetry<T, U>(
      Func<string, IDictionary<string, string>, Task<List<U>>> getList,
      Func<U, bool> itemSelector,
      string listUid, IDictionary<string, string> customHeaders = null)
    {
      var item = await GetItemFromList(listUid, getList, itemSelector, customHeaders);
      if (item == null)
      {
        ClearCacheItem<T>(listUid, null);
        item = await GetItemFromList(listUid, getList, itemSelector, customHeaders);
      }

      return item;
    }
  }
}