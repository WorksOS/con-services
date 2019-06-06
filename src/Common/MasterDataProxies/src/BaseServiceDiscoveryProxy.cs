using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.Proxy.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Service Discovery enabled Proxy
  /// For now, we inherit from the BaseProxy to get code related to caching
  /// But we should create brand new fetch methods than don't accept URL values
  /// As these should be 'resolved' by the Service Resolution class
  /// </summary>
  public abstract class BaseServiceDiscoveryProxy : BaseProxy, IServiceDiscoveryProxy
  {
    private readonly IWebRequest webRequest;
    private readonly IServiceResolution serviceResolution;
    private const int DefaultLogMaxchar = 1000;
    private readonly int logMaxChar = DefaultLogMaxchar;

    protected BaseServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(configurationStore, logger, dataCache)
    {
      this.webRequest = webRequest;
      this.serviceResolution = serviceResolution;
      logMaxChar = configurationStore.GetValueInt("LOG_MAX_CHAR", DefaultLogMaxchar);
    }

    #region Properties

    /// <summary>
    /// Is this Service Local to us?
    /// If the service is near this service in terms of network layout and doesn't need to go via TPaaS or other
    /// When the Service is Inside our Authentication Boundary then we will pass extra authentication information that would normally be added by the Authentication layer if it were an external request
    /// </summary>
    public abstract bool IsInsideAuthBoundary{ get; }

    /// <summary>
    /// The service this proxy is for, if we are accessing an internal (inside our authentication) service
    /// </summary>
    public abstract ApiService InternalServiceType { get; }

    /// <summary>
    /// If we are not accessing an internal service, this variable will be used for the service discovery
    /// </summary>
    public abstract string ExternalServiceName { get; }

    /// <summary>
    /// The version of the API this proxy is for
    /// </summary>
    public abstract ApiVersion Version { get; }

    /// <summary>
    /// The Type of API this service is for, public means it is exposed via TPaaS, so the URL includes /api/version/endpoint
    /// </summary>
    public abstract ApiType Type { get; }

    /// <summary>
    /// If we have a specific cache key for the expiry for cached items
    /// </summary>
    public abstract string CacheLifeKey { get; }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Execute a Post to an endpoint, and cache the result
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>
    protected Task<T> GetMasterDataItemServiceDiscovery<T>(string route, string uid, string userId, IDictionary<string, string> customHeaders,
        IDictionary<string, string> queryParameters = null) 
      where T : class, IMasterDataModel
    {
      return WithMemoryCacheExecute(uid, userId, CacheLifeKey, customHeaders,
        () => RequestAndReturnData<T>(customHeaders, HttpMethod.Get, route, queryParameters));
    }

    /// <summary>
    /// Execute a Post to an endpoint, and cache the result
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>
    protected Task<T> PostMasterDataItemServiceDiscovery<T>(string route, string uid, string userId, IDictionary<string, string> customHeaders,
      IDictionary<string, string> queryParameters = null, Stream payload = null)
      where T : class, IMasterDataModel
    {
      return WithMemoryCacheExecute(uid, userId, CacheLifeKey, customHeaders,
        () => RequestAndReturnData<T>(customHeaders, HttpMethod.Post, route, queryParameters, payload));
    }

    protected Task<T> GetMasterDataItemServiceDiscoveryNoCache<T>(string route, IDictionary<string, string> customHeaders,
      IDictionary<string, string> queryParameters = null) 
      where T : class, IMasterDataModel
    {
        return RequestAndReturnData<T>(customHeaders, HttpMethod.Get, route, queryParameters);
    }

    protected Task<Stream> GetMasterDataStreamItemServiceDiscoveryNoCache(string route, IDictionary<string, string> customHeaders,
      IDictionary<string, string> queryParameters = null, string payload = null)
    {
      return RequestAndReturnDataStream(customHeaders, HttpMethod.Get, route, queryParameters, payload);
    }

    protected Task<T> PostMasterDataItemServiceDiscoveryNoCache<T>(string route, IDictionary<string, string> customHeaders,
      IDictionary<string, string> queryParameters = null, Stream payload = null)
      where T : class, IMasterDataModel
    {
      return RequestAndReturnData<T>(customHeaders, HttpMethod.Post, route, queryParameters, payload);
    }

    protected Task<T> PutMasterDataItemServiceDiscoveryNoCache<T>(string route, IDictionary<string, string> customHeaders,
      IDictionary<string, string> queryParameters = null, Stream payload = null)
      where T : class, IMasterDataModel
    {
      return RequestAndReturnData<T>(customHeaders, HttpMethod.Put, route, queryParameters, payload);
    }

    /// <summary>
    /// Execute a Post/Put/Delete to an endpoint, do not cache the result, and return a ContractExecutionResult
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>
    protected Task<ContractExecutionResult> MasterDataItemServiceDiscoveryNoCache(string route, IDictionary<string, string> customHeaders,
      HttpMethod method, IDictionary<string, string> queryParameters = null, Stream payload = null)
    {
      return RequestAndReturnResult(customHeaders, method, route, queryParameters, payload);
    }

    #endregion

    #region Private Methods

    private Task<string> GetUrl(string route = null, IDictionary<string, string> queryParameters = null)
    {
      if (IsInsideAuthBoundary && InternalServiceType == ApiService.None)
        throw new ArgumentException($"{nameof(InternalServiceType)} has not been defined, it is required for Services Inside our Authentication Boundary");

      if (!IsInsideAuthBoundary && string.IsNullOrEmpty(ExternalServiceName))
        throw new ArgumentException($"{nameof(ExternalServiceName)} has not been defined, it is required for Remote Services");

      return (IsInsideAuthBoundary
        ? serviceResolution.ResolveLocalServiceEndpoint(InternalServiceType, Type, Version, route, queryParameters)
        : serviceResolution.ResolveRemoteServiceEndpoint(ExternalServiceName, Type, Version, route, queryParameters));
    }

    private async Task<Stream> RequestAndReturnDataStream(IDictionary<string, string> customHeaders,
     HttpMethod method, string route = null, IDictionary<string, string> queryParameters = null, string payload = null)  
     //where T : class, IMasterDataModel
    {
      var url = await GetUrl(route, queryParameters);

      // If we are calling to our own services, keep the JWT assertion
      customHeaders.StripHeaders(IsInsideAuthBoundary);

      var streamPayload = payload != null ? new MemoryStream(Encoding.UTF8.GetBytes(payload)) : null;
      var result = await (await webRequest.ExecuteRequestAsStreamContent(url, method, customHeaders, streamPayload)).ReadAsStreamAsync();
      BaseProxyHealthCheck.SetStatus(true, this.GetType());

      log.LogDebug($"{nameof(RequestAndReturnDataStream)} Result: {JsonConvert.SerializeObject(result).Truncate(logMaxChar)}");
      return result;
    }

    private async Task<TResult> RequestAndReturnData<TResult>(IDictionary<string, string> customHeaders,
      HttpMethod method, string route = null, IDictionary<string, string> queryParameters = null, System.IO.Stream payload = null) where TResult : class, IMasterDataModel
    {
      var url = await GetUrl(route, queryParameters);

      // If we are calling to our own services, keep the JWT assertion
      customHeaders.StripHeaders(IsInsideAuthBoundary);

      var result = await webRequest.ExecuteRequest<TResult>(url, payload: payload, customHeaders: customHeaders, method: method);
      log.LogDebug($"{nameof(RequestAndReturnData)} Result: {JsonConvert.SerializeObject(result).Truncate(logMaxChar)}");

      return result;
    }

    private async Task<ContractExecutionResult> RequestAndReturnResult(IDictionary<string, string> customHeaders,
      HttpMethod method, string route = null, IDictionary<string, string> queryParameters = null, System.IO.Stream payload = null) 
    {
      var url = await GetUrl(route, queryParameters);

      // If we are calling to our own services, keep the JWT assertion
      customHeaders.StripHeaders(IsInsideAuthBoundary);

      var result = await webRequest.ExecuteRequest<ContractExecutionResult>(url, payload: payload, customHeaders: customHeaders, method: method);
      log.LogDebug($"{nameof(RequestAndReturnResult)} Result: {JsonConvert.SerializeObject(result)}");

      return result;
    }


    #endregion
  }
}
