using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;

namespace VSS.Productivity3D.TagFileAuth.Proxy
{
  public class TagFileAuthProjectV2ServiceDiscoveryProxy : BaseServiceDiscoveryProxy, ITagFileAuthProjectProxy
  {
    public TagFileAuthProjectV2ServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.TagFileAuth;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V2;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "TAGFILEAUTH_CACHE_LIFE"; // not used


    public async Task<GetProjectAndAssetUidsResult> GetProjectAndAssetUids(GetProjectAndAssetUidsRequest request,
      IDictionary<string, string> customHeaders = null)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      log.LogDebug($"{nameof(GetProjectAndAssetUids)}  getProjectAndAssetUidsRequest: {jsonData}");
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        return await PostMasterDataItemServiceDiscoveryNoCache<GetProjectAndAssetUidsResult>($"project/getUids", customHeaders, payload: payload);
      }
    }

    public async Task<GetProjectAndAssetUidsResult> GetProjectUid(GetProjectUidRequest request,
      IDictionary<string, string> customHeaders = null)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      log.LogDebug($"{nameof(GetProjectUid)}  GetProjectUidRequest: {jsonData}");
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        return await PostMasterDataItemServiceDiscoveryNoCache<GetProjectAndAssetUidsResult>($"project/getUid", customHeaders, payload: payload);
      }
    }
  }

}
