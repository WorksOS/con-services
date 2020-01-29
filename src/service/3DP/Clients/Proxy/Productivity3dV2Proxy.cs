using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;

namespace VSS.Productivity3D.Productivity3D.Proxy
{
  public abstract class Productivity3dV2Proxy : BaseServiceDiscoveryProxy, IProductivity3dV2Proxy
  {
    public Productivity3dV2Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// Execute a generic request against v2 productivity3D endpoint
    /// </summary>
    public async Task<T> ExecuteGenericV2Request<T>(string route, HttpMethod method, Stream body = null, IDictionary<string, string> customHeaders = null, int? timeout = null)
      where T : class, IMasterDataModel
    {
      log.LogDebug($"{nameof(ExecuteGenericV2Request)} route: {route}");

      var response = await SendMasterDataItemServiceDiscoveryNoCache<T>(route, customHeaders, method: method, payload: body, timeout: timeout);
      log.LogDebug($"{nameof(ExecuteGenericV2Request)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      return response;
    }
  }
}
