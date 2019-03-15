using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;


namespace VSS.AssetService.Proxy
{
  /// <summary>
  ///    https://api-stg.trimble.com/t/trimble.com/vss-alpha-assetservice/1.0/Asset/List?customerUid=ead5f851-44c5-e311-aa77-00505688274d&pageSize=200000
  /// </summary>
  public class AssetV1ServiceDiscoveryProxy : BaseServiceDiscoveryProxy, IAssetServiceProxy
  {
    private const string DEFAULT_ASSET_SERVICE_PAGESIZE = "200000";

    public AssetV1ServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => false;

    public override ApiService InternalServiceType => ApiService.None;

    // todoJeannie, for now, need to define "asset-service-public-v1" in appsettings.json
    // "https://api-stg.trimble.com/t/trimble.com/vss-alpha-assetservice/1.0";
    // "https://localhost:5001/api/v1/mock"

    public override string ExternalServiceName => "asset-service";

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "ASSET_CACHE_LIFE";

    /*
       this assetService endpoint is the only one with legacyAssetId
         which is used in 3dp (temporarily) to map between this and Uid for Raptor/TRex
       https://api-stg.trimble.com/t/trimble.com/vss-alpha-assetservice/1.0/Asset/List?customerUid=ead5f851-44c5-e311-aa77-00505688274d&pageSize=200000

     [
      {
          "AssetUID": "069e150c-b606-e311-9e53-0050568824d7",
          "AssetName": null,
          "LegacyAssetID": 930685523290321,
          "SerialNumber": "JMS05073",
          "MakeCode": "CAT",
          "Model": "980H",
          "AssetTypeName": "WHEEL LOADERS",
          "EquipmentVIN": null,
          "IconKey": 27,
          "ModelYear": 2009
      },
      {
          "AssetUID": "bd4333e8-1f21-e311-9ee2-00505688274d",
          "AssetName": null,
          "LegacyAssetID": 1721686110402429,
          "SerialNumber": "LCF00100",
          "MakeCode": "CAT",
          "Model": "CP68B",
          "AssetTypeName": "VIBRATORY SINGLE DRUM PAD",
          "EquipmentVIN": null,
          "IconKey": 88,
          "ModelYear": 0
      }
     ]
    */
    public async Task<List<AssetData>> GetAssetsV1(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var queryParams = new Dictionary<string, string>();
      queryParams.Add("customerUid", customerUid);
      queryParams.Add("pageSize", DEFAULT_ASSET_SERVICE_PAGESIZE);
      var result =
        await GetMasterDataItemServiceDiscovery<AssetDataResult>("/Asset/List", customerUid, null, customHeaders,
          queryParams);
      if (result.Code == 0)
      {
        return result.Assets;
      }

      log.LogDebug($"Failed to get list of assets: {result.Code}, {result.Message}");
      return null;
    }


    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (either customerUid or assetUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId = null)
    {
      ClearCacheByTag(uid);

      if (string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }
  }
}
