using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;

namespace VSS.Productivity3D.Productivity3D.Proxy
{
  /// <summary>
  /// Used to execute the Base ExecuteGenericV2Request() method for 3dp tag file controller in VSS
  /// </summary>
  public class GenericHttpProxy : BaseServiceDiscoveryProxy, IGenericHttpProxy
  {
    public override bool IsInsideAuthBoundary => false; // todoJeannie how to ignore

    public override ApiService InternalServiceType => ApiService.None; // todoJeannie how to ignore

    public override string ExternalServiceName => null; // todoJeannie how to ignore

    public override ApiVersion Version => ApiVersion.V1; // todoJeannie how to ignore

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "GENERIC_HTTP_CACHE_LIFE"; // not used

    public GenericHttpProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// Execute a generic request against supplied url and endpoint
    /// </summary>
    public async Task<T> ExecuteGenericHttpRequest<T>(string url, HttpMethod method, Stream body = null, IDictionary<string, string> customHeaders = null, int? timeout = null)
      where T : class, IMasterDataModel
    {
      log.LogDebug($"{nameof(ExecuteGenericHttpRequest)} url: {url}");

      var response = await SendMasterDataItemServiceDiscoveryGenericHttpNoCache<T>(url, customHeaders, method: method, payload: body, timeout: timeout);
      log.LogDebug($"{nameof(ExecuteGenericHttpRequest)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      return response;
    }

  }
}
