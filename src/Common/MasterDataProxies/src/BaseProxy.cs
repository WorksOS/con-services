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
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  ///   Base class for proxies getting master data from services.
  /// </summary>
  public class BaseProxy
  {
    private readonly IDataCache dataCache;
    private readonly static AsyncDuplicateLock memCacheLock = new AsyncDuplicateLock();
    protected readonly IConfigurationStore configurationStore;
    protected readonly ILogger log;
    protected readonly ILoggerFactory logger;
    
    private const int DefaultLogMaxChar = 1000;
    protected readonly int _logMaxChar;

    protected BaseProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache)
    {
      log = logger.CreateLogger<BaseProxy>();
      this.logger = logger;
      this.configurationStore = configurationStore;
      this.dataCache = dataCache;
      _logMaxChar = configurationStore.GetValueInt("LOG_MAX_CHAR", DefaultLogMaxChar);
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
      _logMaxChar = configurationStore.GetValueInt("LOG_MAX_CHAR", DefaultLogMaxChar);
    }

    private async Task<T> SendRequestInternal<T>(string url, IDictionary<string, string> customHeaders,
      HttpMethod method = null, string payload = null, Stream streamPayload = null, int? timeout = null, int retries = 3)
    {
      // Default to POST
      if (method == null)
        method = HttpMethod.Post;
      var result = default(T);
      log.LogDebug($"{nameof(SendRequestInternal)}: Preparing {url} ({method}) headers {customHeaders.LogHeaders(_logMaxChar)}");
      try
      {
        var request = new GracefulWebRequest(logger, configurationStore);
        if (method != HttpMethod.Get)
        {
          if (streamPayload != null && payload == null)
            result = await request.ExecuteRequest<T>(url, streamPayload, customHeaders, method, timeout, retries);
          else
          {
            if (payload != null)
            {
              using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
              {
                result = await request.ExecuteRequest<T>(url, ms, customHeaders, method, timeout, retries);
              }
            }
          }
        }
        else
        {
          result = await request.ExecuteRequest<T>(url, method: HttpMethod.Get,customHeaders: customHeaders, timeout:timeout, retries:retries);
        }

        log.LogDebug($"{nameof(SendRequestInternal)}: Result of send to master data request: {JsonConvert.SerializeObject(result).Truncate(_logMaxChar)}");
        BaseProxyHealthCheck.SetStatus(true,this.GetType());
      }
      catch (Exception ex)
      {
        LogWebRequestExceptionAndSetHealth(ex);
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
    /// <param name="timeout">Optional timeout in milliseconds for the request</param>
    /// <param name="retries">How many times to retry the request (optional)</param>
    /// <returns>The item</returns>
    protected Task<T> SendRequest<T>(string urlKey, string payload, IDictionary<string, string> customHeaders,
      string route = null, HttpMethod method = null, string queryParameters = null, int ? timeout = null, int retries = 3)
    {
      log.LogDebug($"{nameof(SendRequest)}: Executing {urlKey} ({method}) {route} {queryParameters.Truncate(_logMaxChar)} {payload.Truncate(_logMaxChar)} {customHeaders.LogHeaders(_logMaxChar)}");
      return SendRequestInternal<T>(ExtractUrl(urlKey, route, queryParameters), customHeaders, method, payload, timeout:timeout, retries:retries);
    }

    /// <summary>
    ///   Executes a request against master data service
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <param name="method">Http method, defaults to POST</param>
    /// <param name="queryParameters">Query parameters (optional)</param>
    /// <returns>The item</returns>
    protected Task<T> SendRequest<T>(string urlKey, string payload, IDictionary<string, string> customHeaders,
      string route = null, HttpMethod method = null, IDictionary<string, string> queryParameters = null)
    {
      log.LogDebug($"{nameof(SendRequest)}: Executing {urlKey} ({method}) {route} {queryParameters.LogHeaders(_logMaxChar)} {payload.Truncate(_logMaxChar)} {customHeaders.LogHeaders(_logMaxChar)}");
      return SendRequestInternal<T>(ExtractUrl(urlKey, route, queryParameters), customHeaders, method, payload);
    }

    /// <summary>
    ///   Executes a request against master data service
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="payload">The payload of the request</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <param name="method">Http method, defaults to POST</param>
    /// <returns>The item</returns>
    protected Task<T> SendRequest<T>(string urlKey, Stream payload, IDictionary<string, string> customHeaders,
      string route = null, HttpMethod method = null, IDictionary<string, string> queryParameters = null)
    {
      return SendRequestInternal<T>(ExtractUrl(urlKey, route, queryParameters), customHeaders, method,
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
    protected Task<T> SendRequest<T>(string urlKey, Stream payload, IDictionary<string, string> customHeaders,
      string route = null, IDictionary<string, string> queryParams = null, HttpMethod method = null)
    {
      return SendRequestInternal<T>(ExtractUrl(urlKey, route, string.Empty), customHeaders, method,
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
        result = await request.ExecuteRequest<K>(url, customHeaders: customHeaders, method: HttpMethod.Get);
        log.LogDebug($"{nameof(GetObjectsFromMasterdata)}: Result of get item request: {JsonConvert.SerializeObject(result).Truncate(_logMaxChar)}");
        BaseProxyHealthCheck.SetStatus(true,this.GetType());
      }
      catch (Exception ex)
      {
        LogWebRequestExceptionAndSetHealth(ex);
        throw;
      }

      return result;
    }


    /// <summary>
    ///   Gets an item from the specified service. No caching.
    /// </summary>
    /// <param name="urlKey">The configuration store key for the URL</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <param name="queryParams">Query parameters for the request (optional)</param>
    /// <param name="route">Additional routing to add to the base URL (optional)</param>
    /// <returns>List of items</returns>
    protected Task<T> GetMasterDataItem<T>(string urlKey, IDictionary<string, string> customHeaders,
      string queryParams = null, string route = null)
    {
      return GetObjectsFromMasterdata<T>(urlKey, customHeaders, queryParams, route);
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
      IDictionary<string, string> customHeaders, HttpMethod method = null, string payload = null,
      string queryParams = null, string route = null)
    {
      Stream result = null;
      var url = ExtractUrl(urlKey, route, queryParams);
      try
      {
        if (method == null)
          method = HttpMethod.Get;

        var request = new GracefulWebRequest(logger, configurationStore);
        if (method != HttpMethod.Get)
        {
          var streamPayload = payload != null ? new MemoryStream(Encoding.UTF8.GetBytes(payload)) : null;
          result = await (await request.ExecuteRequestAsStreamContent(url, method, customHeaders, streamPayload)).ReadAsStreamAsync();
        }
        else
          result = await (await request.ExecuteRequestAsStreamContent(url, HttpMethod.Get, customHeaders)).ReadAsStreamAsync();
        BaseProxyHealthCheck.SetStatus(true, this.GetType());
      }
      catch (Exception ex)
      {
        LogWebRequestExceptionAndSetHealth(ex);
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
    protected Task<T> GetMasterDataItem<T>(string uid, string userId, string cacheLifeKey, string urlKey,
      IDictionary<string, string> customHeaders, string route = null) where T : class, IMasterDataModel
    {
      return WithMemoryCacheExecute(uid, userId, cacheLifeKey, customHeaders,
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
    protected async Task<T> WithMemoryCacheExecute<T>(string uid, string userId, object cacheLifeKey, IDictionary<string, string> customHeaders, Func<Task<T>> action) where T : class, IMasterDataModel
    {
      if (dataCache == null)
        throw new InvalidOperationException("This method requires a cache; use the correct constructor");

      if (string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(userId))
      {
        log.LogWarning("Attempting to execute method with cache, but cannot generate a cache key - not caching the result.");
        var noCacheResult = await action.Invoke();
        if (noCacheResult != null)
          return noCacheResult;
      }
      else
      {
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

        T result = default(T);

        using (await memCacheLock.LockAsync(cacheKey))
        {
          if (!IfCacheNeedsToBeInvalidated(customHeaders))
          {
            return await dataCache.GetOrCreate(cacheKey, async entry =>
            {
              entry.SetOptions(opts);
              log.LogDebug($"{nameof(WithMemoryCacheExecute)}: Item for key {cacheKey} not found in cache, getting from web api");
              result = await action.Invoke();
              if (result != null)
                return new CacheItem<T>(result, result.GetIdentifiers());

              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                  "Unable to request data from a webapi"));

            });
          }

          log.LogDebug($"{nameof(WithMemoryCacheExecute)}: Item for key {cacheKey} is requested to be invalidated, getting from web api");
          result = await action.Invoke();
          if (result != null)
            return dataCache.Set(cacheKey, result, result.GetIdentifiers(), opts);
        }
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Unable to request data from a webapi"));
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
    protected Task<T> GetMasterDataItem<T>(string uid, string userId, TimeSpan cacheLifeKey, string urlKey,
      IDictionary<string, string> customHeaders, string route = null) where T : class, IMasterDataModel
    {
      return WithMemoryCacheExecute(uid, userId, cacheLifeKey, customHeaders,
        () => GetMasterDataItem<T>(urlKey, customHeaders, null, route));
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
    protected Task<T> GetContainedMasterDataList<T>(string uid, string userId, string cacheLifeKey, string urlKey,
      IDictionary<string, string> customHeaders, string queryParams = null, string route = null) where T : class, IMasterDataModel
    {
      return WithMemoryCacheExecute(uid, userId, cacheLifeKey, customHeaders,
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
      log.LogInformation($"{nameof(ExtractBaseUrl)}: {urlKey}: {url}, route={route}");

      if (string.IsNullOrEmpty(url))
      {
        var errorString = $"Your application is missing an environment variable, urlKey: {urlKey}";
        log.LogError($"{nameof(ExtractBaseUrl)}: error: {errorString}");
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
      if (customHeaders == null)
        return false;
      customHeaders.TryGetValue("X-VisionLink-ClearCache", out var caching);
      return !string.IsNullOrEmpty(caching) && caching == "true";
    }

    /// <summary>
    ///   Clears an item from the cache
    /// </summary>
    /// <typeparam name="T">The type of item being cached</typeparam>
    /// <param name="uid">The uid of the item to remove from the cache</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    [Obsolete("Used the ClearCacheByTag method, which isn't dependent on the Type T")]
    protected void ClearCacheItem<T>(string uid, string userId)
    {
      if (dataCache == null)
        throw new InvalidOperationException("This method requires a cache; use the correct constructor");
      var cacheKey = GetCacheKey<T>(uid, userId);
      log.LogDebug($"{nameof(ClearCacheItem)}: Clearing item from cache: {cacheKey}");
      dataCache.RemoveByKey(cacheKey);
    }

    /// <summary>
    /// Clear any cached items that related the UID passed in
    /// </summary>
    protected void ClearCacheByTag(string uid)
    {
      if(!string.IsNullOrEmpty(uid))
        dataCache.RemoveByTag(uid);
    }

    /// <summary>
    /// Gets an item from a list. (Customer based)
    /// </summary>
    /// <typeparam name="U">The type of item in the list</typeparam>
    /// <param name="listUid">The uid for the get request, also the cache key</param>
    /// <param name="getList">The method to call to get the list of items</param>
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
    /// Gets an item from a list. (User based)
    /// </summary>
    /// <typeparam name="U">The type of item in the list</typeparam>
    /// <param name="listUid">The uid for the get request, also the cache key</param>
    /// <param name="userId">The user uid</param>
    /// <param name="getList">The method to call to get the list of items</param>
    /// <param name="itemSelector">The predicate to select the required item from the list</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <returns>The item</returns>
    private async Task<U> GetItemFromList<U>(string listUid, string userId,
      Func<string, string, IDictionary<string, string>, Task<List<U>>> getList,
      Func<U, bool> itemSelector,
      IDictionary<string, string> customHeaders = null)
    {
      var list = await getList(listUid, userId, customHeaders);
      return list.SingleOrDefault(itemSelector);
    }

    /// <summary>
    ///   Check exception for Web Request details and log a warning
    /// </summary>
    /// <param name="ex">Exception to be logged</param>
    private void LogWebRequestExceptionAndSetHealth(Exception ex)
    {
      var message = ex.Message;
      var stacktrace = ex.StackTrace;
      //Check for 400 and 500 errors which come through as an inner exception
      if (ex.InnerException != null)
      {
        message = ex.InnerException.Message;
        stacktrace = ex.InnerException.StackTrace;
      }

      log.LogWarning($"{nameof(LogWebRequestExceptionAndSetHealth)}: Error sending data from master data: ", message);
      log.LogWarning($"{nameof(LogWebRequestExceptionAndSetHealth)}: Stacktrace: ", stacktrace);

      //WE want to exclude business exceptions as they are valid cases for health monitoring
      if (ex.InnerException != null && ex.InnerException is ServiceException serviceException &&
          serviceException.Code == HttpStatusCode.BadRequest)
        return;
      BaseProxyHealthCheck.SetStatus(false, this.GetType());
    }

    /// <summary>
    /// Gets an item from a customer based list. If the item is not in the list then clears the cache and does the get again
    /// which will issue the http request to get the list again. This lets us pick up items which have been
    /// added since the list was cached (default 15 mins).
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

    /// <summary>
    /// Gets an item from a user based list. If the item is not in the list then clears the cache and does the get again
    /// which will issue the http request to get the list again. This lets us pick up items which have been
    /// added since the list was cached (default 15 mins).
    /// </summary>
    /// <typeparam name="T">The type of the result from the http request</typeparam>
    /// <typeparam name="U">The type of item in the list</typeparam>
    /// <param name="getList">The method to call to get the list of items.</param>
    /// <param name="itemSelector">The predicate to select the required item from the list</param>
    /// <param name="listUid">The uid for the get request, also the cache key</param>
    /// <param name="userId">The user uid</param>
    /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
    /// <returns></returns>
    public async Task<U> GetItemWithRetry<T, U>(
      Func<string, string, IDictionary<string, string>, Task<List<U>>> getList,
      Func<U, bool> itemSelector, string listUid, 
      string userId, IDictionary<string, string> customHeaders = null)
    {
      var item = await GetItemFromList(listUid, userId, getList, itemSelector, customHeaders);
      if (item == null)
      {
        ClearCacheItem<T>(listUid, userId);
        item = await GetItemFromList(listUid, userId, getList, itemSelector, customHeaders);
      }

      return item;
    }
  }
}
