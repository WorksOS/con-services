using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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
    public async Task<TResponse> SendDataPostRequest<TResponse, TRequest>(TRequest dataRequest, string route, IDictionary<string, string> customHeaders = null, bool mutableGateway = false) where TResponse : ContractExecutionResult
    {
      var request = JsonConvert.SerializeObject(dataRequest);

      log.LogDebug($"{nameof(TRequest)}: Sending the request: {request}");

      var response = await SendRequest<TResponse>(mutableGateway ? TREX_GATEWAY_MUTABLE_BASE_URL : TREX_GATEWAY_IMMUTABLE_BASE_URL, request, customHeaders, route, HttpMethod.Post, string.Empty);

      log.LogDebug($"{nameof(SendDataPostRequest)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    /// <summary>
    /// Sends a request to get data as a stream from the TRex immutable database.
    /// </summary>
    public Task<Stream> SendDataPostRequestWithStreamResponse<TRequest>(TRequest dataRequest, string route, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(dataRequest);

      log.LogDebug($"{nameof(TRequest)}: Sending the request: {request}");

      var result = GetMasterDataStreamContent(TREX_GATEWAY_IMMUTABLE_BASE_URL, customHeaders, HttpMethod.Post, request, null, route);

      return result;
    }

    /// <summary>
    /// Sends a request to delete data to the TRex immutable/mutable database.
    /// </summary>
    public async Task<TResponse> SendDataDeleteRequest<TResponse, TRequest>(TRequest dataRequest, string route, IDictionary<string, string> customHeaders = null, bool mutableGateway = false) where TResponse : ContractExecutionResult
    {
      var request = JsonConvert.SerializeObject(dataRequest);

      log.LogDebug($"{nameof(TRequest)}: Sending the request: {request}");

      var response = await SendRequest<TResponse>(mutableGateway ? TREX_GATEWAY_MUTABLE_BASE_URL : TREX_GATEWAY_IMMUTABLE_BASE_URL, request, customHeaders, route, HttpMethod.Delete, string.Empty);

      log.LogDebug($"{nameof(SendDataDeleteRequest)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    /// <summary>
    /// Sends a request to get site model data from the TRex immutable database.
    /// </summary>
    public async Task<TResponse> SendDataGetRequest<TResponse>(string siteModelId, string route, IDictionary<string, string> customHeaders = null, IDictionary<string, string> queryParameters = null)
      where TResponse : class, IMasterDataModel
    {
      log.LogDebug($"{nameof(TResponse)}: Sending the get data request for site model ID: {siteModelId}");

      var response = await SendRequest<TResponse>(TREX_GATEWAY_IMMUTABLE_BASE_URL, string.Empty, customHeaders, route, HttpMethod.Get, queryParameters);

      log.LogDebug($"{nameof(SendDataGetRequest)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

  }
}
