using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class FleetAssetSummaryProxy : BaseServiceDiscoveryProxy, IFleetAssetSummaryProxy
  {
    public FleetAssetSummaryProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    {
    }
    //assetsummary-public-v1_1 = https://unifiedfleet.myvisionlink.com/t/trimble.com/vss-assetutilization/1.1

    //https://unifiedfleet.myvisionlink.com/t/trimble.com/vss-assetutilization/1.1/ details/summary?assetUid=ace44294-634e-e311-b5a0-90e2ba076184&date=04/02/19

    public override bool IsInsideAuthBoundary => false;
    public override ApiService InternalServiceType => ApiService.None;
    public override string ExternalServiceName => "assetsummary";
    public override ApiVersion Version => ApiVersion.V1;
    public override ApiType Type => ApiType.Public;
    public override string CacheLifeKey => "FLEET_ASSET_SUMMARY_CACHE_LIFE";

    public Task<AssetSummary> GetAssetSummary(string assetUid, IDictionary<string, string> customHeaders = null)
    {
      return GetMasterDataItemServiceDiscovery<AssetSummary>("details/summary", assetUid, null,
        customHeaders,
        new List<KeyValuePair<string, string>>{new KeyValuePair<string, string>("assetUid", assetUid),
          new KeyValuePair<string, string>( "date",DateTime.UtcNow.ToString("M/d/yy"))
        });
    }
  }
}
