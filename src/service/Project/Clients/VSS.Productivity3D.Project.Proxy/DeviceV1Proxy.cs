using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Proxy
{
  public class DeviceV1Proxy : BaseServiceDiscoveryProxy, IDeviceProxy
  {
    public DeviceV1Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public  override bool IsInsideAuthBoundary => true;

    public  override ApiService InternalServiceType => ApiService.Device;

    public override string ExternalServiceName => null;

    public  override ApiVersion Version => ApiVersion.V1;

    public  override ApiType Type => ApiType.Public;

    public  override string CacheLifeKey => "DEVICE_CACHE_LIFE";
     

    public async Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<Guid> deviceUids, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMatchingDevices)} deviceUids: {deviceUids}");
      if (deviceUids.Count == 0)
        return new DeviceMatchingModel().deviceIdentifiers;

      using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deviceUids))))
      {
        var result = await SendMasterDataItemServiceDiscoveryNoCache<DeviceMatchingModel>("/device/deviceuids", customHeaders, HttpMethod.Post, payload: ms);
        if (result.Code == 0)
          return result.deviceIdentifiers;

        log.LogDebug($"{nameof(GetMatchingDevices)} Failed to get list of devices: {result.Code}, {result.Message}");
      }

      return null;
    }

    public async Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<long> shortRaptorAssetIds, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMatchingDevices)} shortRaptorAssetIds: {shortRaptorAssetIds}");
      if (shortRaptorAssetIds.Count == 0)
        return new DeviceMatchingModel().deviceIdentifiers;

      using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(shortRaptorAssetIds))))
      {
        var result = await SendMasterDataItemServiceDiscoveryNoCache<DeviceMatchingModel>("/device/shortRaptorAssetIds", customHeaders, HttpMethod.Post, payload: ms);
        if (result.Code == 0)
          return result.deviceIdentifiers;

        log.LogDebug($"{nameof(GetMatchingDevices)} Failed to get list of devices: {result.Code}, {result.Message}");
      }

      return null;
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (deviceUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId = null)
    {
      ClearCacheByTag(uid);

      if (string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }
  }
}
