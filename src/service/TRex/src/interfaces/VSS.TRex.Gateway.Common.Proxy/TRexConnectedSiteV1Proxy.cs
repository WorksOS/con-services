using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Proxy for TRex tag files and connected service.
  /// </summary>
  public class TRexConnectedSiteV1Proxy : BaseTRexServiceDiscoveryProxy, ITRexConnectedSiteProxy
  {
    public TRexConnectedSiteV1Proxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.None;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "TREX_CONNECTEDSITE_CACHE_LIFE"; // not used

    
    /// <summary>
    /// Sends a tag file to TRex for to send to connectedSite
    /// </summary>  
    public async Task<ContractExecutionResult> SendTagFileNonDirectToConnectedSite(CompactionTagFileRequest compactionTagFileRequest,
      IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(SendTagFileNonDirectToConnectedSite)}: Filename: {compactionTagFileRequest.FileName}");
      Gateway = GatewayType.ConnectedSite;
      return await SendTagFileRequest(compactionTagFileRequest, customHeaders, HttpMethod.Post, "/status");
    }

    private async Task<ContractExecutionResult> SendTagFileRequest(CompactionTagFileRequest compactionTagFileRequest,
      IHeaderDictionary customHeaders, HttpMethod method, string route)
    {
      var jsonData = JsonConvert.SerializeObject(compactionTagFileRequest);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
        return await MasterDataItemServiceDiscoveryNoCache<ContractExecutionResult>(route, customHeaders, method, payload: payload);
    }
  }
}
