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
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Proxy to access the TRex Gateway WebAPIs.
  /// </summary>
  public class TRexCompactionDataV1ServiceDiscoveryProxy : BaseTRexServiceDiscoveryProxy, ITRexCompactionDataProxy
  {
    public TRexCompactionDataV1ServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.None;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "TREX_COMPACTIONDATA_CACHE_LIFE"; // not used


    /// <summary>
    /// Sends a request to get/save data from/to the TRex immutable/mutable database.
    /// </summary>
    public async Task<TResponse> SendDataPostRequest<TResponse, TRequest>(TRequest dataRequest, string route,
      IDictionary<string, string> customHeaders = null, bool mutableGateway = false)
      where TResponse : ContractExecutionResult
    {
      Gateway = mutableGateway ? GatewayType.Mutable : GatewayType.Immutable;
      var jsonData = JsonConvert.SerializeObject(dataRequest);
      log.LogDebug($"{nameof(SendDataPostRequest)}: Sending the request: {jsonData.Truncate(logMaxChar)}");

      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
        return (TResponse) await MasterDataItemServiceDiscoveryNoCache(route, customHeaders, HttpMethod.Post, payload: payload);
    }

    /// <summary>
    /// Sends a request to get data as a stream from the TRex immutable database.
    /// </summary>
    public async Task<Stream> SendDataPostRequestWithStreamResponse<TRequest>(TRequest dataRequest, string route,
      IDictionary<string, string> customHeaders = null)
    {
      Gateway = GatewayType.Immutable;
      var payload = JsonConvert.SerializeObject(dataRequest);
      log.LogDebug($"{nameof(SendDataPostRequestWithStreamResponse)}: Sending the request: {payload.Truncate(logMaxChar)}");

      var result = await GetMasterDataStreamItemServiceDiscoveryNoCache
        (route, customHeaders, method: HttpMethod.Put, payload: payload);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(SendDataPostRequestWithStreamResponse)} Failed to get streamed results");
      return null;
    }

    /// <summary>
    /// Sends a request to get site model data from the TRex immutable database.
    /// </summary>
    public async Task<TResponse> SendDataGetRequest<TResponse>(string siteModelId, string route,
      IDictionary<string, string> customHeaders = null)
      where TResponse : class, IMasterDataModel
    {
      Gateway = GatewayType.Immutable;
      log.LogDebug($"{nameof(SendDataGetRequest)}: Sending the get data request for site model ID: {siteModelId}");
      return await GetMasterDataItemServiceDiscoveryNoCache<TResponse>(route, customHeaders);
    }
  }
}
