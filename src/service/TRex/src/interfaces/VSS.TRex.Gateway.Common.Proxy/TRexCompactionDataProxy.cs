using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Proxy to access the TRex Gateway WebAPIs.
  /// </summary>
  [Obsolete("Use TRexCompactionDataV1ServiceDiscoveryProxy instead")]
  public class TRexCompactionDataProxy : BaseProxy, ITRexCompactionDataProxy
  {
    private const string TREX_GATEWAY_IMMUTABLE_BASE_URL = "TREX_GATEWAY_API_URL";
    private const string TREX_GATEWAY_MUTABLE_BASE_URL = "TREX_MUTABLE_GATEWAY_API_URL";

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="configurationStore"></param>
    /// <param name="logger"></param>
    public TRexCompactionDataProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    {
    }

    /// <summary>
    /// Sends a request to get/save data from/to the TRex immutable/mutable database.
    /// </summary>
    /// <param name="dataRequest"></param>
    /// <param name="route"></param>
    /// <param name="customHeaders"></param>
    /// <param name="mutableGateway"></param>
    /// <returns></returns>
    public Task<TResponse> SendDataPostRequest<TResponse, TRequest>(TRequest dataRequest, string route, IDictionary<string, string> customHeaders = null, bool mutableGateway = false) where TResponse : ContractExecutionResult
    {
      var request = JsonConvert.SerializeObject(dataRequest);

      log.LogDebug($"{nameof(TRequest)}: Sending the request: {request}");

      return SendRequestPost<TResponse>(request, customHeaders, route, mutableGateway ? TREX_GATEWAY_MUTABLE_BASE_URL : TREX_GATEWAY_IMMUTABLE_BASE_URL);
    }

    /// <summary>
    /// Sends a request to get data as a stream from the TRex immutable database.
    /// </summary>
    /// <param name="dataRequest"></param>
    /// <param name="route"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<Stream> SendDataPostRequestWithStreamResponse<TRequest>(TRequest dataRequest, string route, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(dataRequest);

      log.LogDebug($"{nameof(TRequest)}: Sending the request: {request}");

      return SendRequestPostAsStreamContent(request, customHeaders, route);
    }

    /// <summary>
    /// Sends a request to get site model data from the TRex immutable database.
    /// </summary>
    /// <param name="siteModelId"></param>
    /// <param name="route"></param>
    /// <param name="customHeaders"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public Task<TResponse> SendDataGetRequest<TResponse>(string siteModelId, string route, IDictionary<string, string> customHeaders = null, IDictionary<string, string> queryParameters = null)
      where TResponse : class, IMasterDataModel
    {
      log.LogDebug($"{nameof(TResponse)}: Sending the get data request for site model ID: {siteModelId}");

      return SendRequestGet<TResponse>(customHeaders, route, queryParameters);
    }

    /// <summary>
    /// Executes a POST request against the TRex Gateway service.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <param name="baseUrl"></param>
    /// <returns></returns>
    private async Task<T> SendRequestPost<T>(string payload, IDictionary<string, string> customHeaders, string route, string baseUrl = TREX_GATEWAY_IMMUTABLE_BASE_URL) where T : ContractExecutionResult
    {
      var response = await SendRequest<T>(baseUrl, payload, customHeaders, route, HttpMethod.Post, string.Empty);

      log.LogDebug($"{nameof(SendRequestPost)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    /// <summary>
    /// Executes a POST request against the TRex Gateway service.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <param name="baseUrl"></param>
    /// <returns></returns>
    private async Task<T> SendRequestPostEx<T>(string payload, IDictionary<string, string> customHeaders, string route, string baseUrl = TREX_GATEWAY_IMMUTABLE_BASE_URL) where T : ActionResult
    {
      var response = await SendRequest<T>(baseUrl, payload, customHeaders, route, HttpMethod.Post, string.Empty);

      log.LogDebug($"{nameof(SendRequestPostEx)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    /// <summary>
    /// Executes a POST request against the TRex Gateway service as stream content.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    private Task<Stream> SendRequestPostAsStreamContent(string payload, IDictionary<string, string> customHeaders, string route)
    {
      var result = GetMasterDataStreamContent(TREX_GATEWAY_IMMUTABLE_BASE_URL, customHeaders, HttpMethod.Post, payload, null, route);

      return result;
    }

    /// <summary>
    /// Executes a GET request against the TRex Gateway service.
    /// </summary>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    private async Task<T> SendRequestGet<T>(IDictionary<string, string> customHeaders, string route, IDictionary<string, string> queryParameters = null)
    {
      var response = await SendRequest<T>(TREX_GATEWAY_IMMUTABLE_BASE_URL, string.Empty, customHeaders, route, HttpMethod.Get, queryParameters);

      log.LogDebug($"{nameof(SendRequestGet)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }
  }
}
