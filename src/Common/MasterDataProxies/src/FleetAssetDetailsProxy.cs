using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class FleetAssetDetailsProxy : BaseServiceDiscoveryProxy, IFleetAssetDetailsProxy
  {
    public override bool IsInsideAuthBoundary => false;
    public override ApiService InternalServiceType => ApiService.None;
    public override string ExternalServiceName => "assetdetails";
    public override ApiVersion Version => ApiVersion.V1;
    public override ApiType Type => ApiType.Public;
    public override string CacheLifeKey => "FLEET_ASSET_DETAILS_CACHE_LIFE";

    public FleetAssetDetailsProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    { }

    public Task<AssetDetails> GetAssetDetails(string assetUid, IHeaderDictionary customHeaders = null)
    {
      return GetMasterDataItemServiceDiscovery<AssetDetails>("UnifiedFleet/AssetDetails/v1", assetUid, null,
        customHeaders, new List<KeyValuePair<string, string>>{new KeyValuePair<string, string>("assetUID", assetUid)});
    }
  }
}
