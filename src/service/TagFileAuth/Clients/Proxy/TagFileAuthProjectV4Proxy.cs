using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Proxy
{
  public class TagFileAuthProjectV4Proxy : BaseServiceDiscoveryProxy, ITagFileAuthProjectProxy
  {
    public TagFileAuthProjectV4Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.TagFileAuth;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V4;

    public override ApiType Type => ApiType.Private;

    public override string CacheLifeKey => "TAGFILEAUTH_CACHE_LIFE"; // not used


    public async Task<GetProjectAndAssetUidsResult> GetProjectAndAssetUids(GetProjectAndAssetUidsRequest request,
      IDictionary<string, string> customHeaders = null)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      log.LogDebug($"{nameof(GetProjectAndAssetUids)}  getProjectAndAssetUidsRequest: {jsonData}");
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        return await SendMasterDataItemServiceDiscoveryNoCache<GetProjectAndAssetUidsResult>($"project/getUids", customHeaders, HttpMethod.Post, payload: payload);
      }
    }

    public async Task<GetProjectAndAssetUidsEarthWorksResult> GetProjectAndAssetUidsEarthWorks(GetProjectAndAssetUidsEarthWorksRequest request,
      IDictionary<string, string> customHeaders = null)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      log.LogDebug($"{nameof(GetProjectAndAssetUidsEarthWorks)}  getProjectAndAssetUidsEarthWorksRequest: {jsonData}");
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        return await SendMasterDataItemServiceDiscoveryNoCache<GetProjectAndAssetUidsEarthWorksResult>($"project/getUidsEarthWorks", customHeaders, HttpMethod.Post, payload: payload);
      }
    }
  }

}
