using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;
using VSS.Productivity3D.AssetMgmt3D.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Proxy
{
  public class AssetResolverProxy : BaseServiceDiscoveryProxy, IAssetResolverProxy
  {
    public AssetResolverProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// THis service support internal calls only
    /// </summary>
    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.AssetMgmt3D;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "ASSETMGMT_CACHE_LIFE";

    public async Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingAssets(List<Guid> assetUids,
      IDictionary<string, string> customHeaders = null)
    {
      if (assetUids.Count == 0)
        return new List<KeyValuePair<Guid, long>>();
      
      var result =
        await PostMasterDataItemServiceDiscovery<AssetDisplayModel>("/assets/assetuids", null, null, customHeaders,
          payload: new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(assetUids))));
      if (result.Code == 0)
      {
        return result.assetIdentifiers;
      }

      log.LogDebug($"Failed to get list of assets (Guid list): {result.Code}, {result.Message}");
      return null;
    }

    public async Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingAssets(List<long> assetIds,
      IDictionary<string, string> customHeaders = null)
    {
      if (assetIds.Count == 0)
        return new List<KeyValuePair<Guid, long>>();

      var result =
        await PostMasterDataItemServiceDiscovery<AssetDisplayModel>("/assets/assetids", null, null, customHeaders,
          payload: new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(assetIds))));
      if (result.Code == 0)
      {
        return result.assetIdentifiers;
      }

      log.LogDebug($"Failed to get list of assets (long list): {result.Code}, {result.Message}");
      return null;
    }

    public async Task<MatchingAssetsDisplayModel> GetMatching3D2DAssets(MatchingAssetsDisplayModel asset,
      IDictionary<string, string> customHeaders = null)
    {
      MatchingAssetsDisplayModel result = null;

      if (!string.IsNullOrEmpty(asset.AssetUID2D))
        result =
          await GetMasterDataItemServiceDiscovery<MatchingAssetsDisplayModel>(
            $"/assets/match2dasset/{asset.AssetUID2D}",
            null, null, customHeaders);

      if (!string.IsNullOrEmpty(asset.AssetUID3D))
        result =
          await GetMasterDataItemServiceDiscovery<MatchingAssetsDisplayModel>(
            $"/assets/match3dasset/{asset.AssetUID3D}",
            null, null, customHeaders);

      if (result == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "No assert to process provided"));

      if (result.Code == 0)
        return result;

      log.LogDebug($"Failed to get list of matching assets: {result.Code}, {result.Message}");
      return null;
    }

    public void ClearCacheItem(string uid, string userId = null)
    {
      throw new NotImplementedException();
    }
  }
}
