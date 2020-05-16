using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Proxy
{
  public class DeviceInternalV1Proxy : BaseServiceDiscoveryProxy, IDeviceInternalProxy
  {
    public DeviceInternalV1Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public  override bool IsInsideAuthBoundary => true;

    public  override ApiService InternalServiceType => ApiService.Device;

    public override string ExternalServiceName => null;

    public  override ApiVersion Version => ApiVersion.V1;

    public  override ApiType Type => ApiType.Private;

    public  override string CacheLifeKey => "DEVICE_INTERNAL_CACHE_LIFE";


    public async Task<DeviceData> GetDevice(string serialNumber, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevice)} serialNumber: {serialNumber}");
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("serialNumber", serialNumber) };
      var result = await GetMasterDataItemServiceDiscoveryNoCache<DeviceDataSingleResult>($"/device/serialnumber",
        //serialNumber, null,
        customHeaders, queryParams);

      log.LogDebug($"{nameof(GetDevice)} get device for serialNumber {serialNumber} result: {result.Code}, {result.Message}");
      return result.DeviceDescriptor;
    }

    public async Task<DeviceData> GetDevice(int shortRaptorAssetId, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevice)} shortRaptorAssetId: {shortRaptorAssetId}");
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("shortRaptorAssetId", shortRaptorAssetId.ToString()) };
      var result = await GetMasterDataItemServiceDiscovery<DeviceDataSingleResult>($"/device/shortRaptorAssetId",
        shortRaptorAssetId.ToString(), null,
        customHeaders, queryParams);

      log.LogDebug($"{nameof(GetDevice)} get device for shortRaptorAssetId {shortRaptorAssetId} result: {result.Code}, {result.Message}");
      return result.DeviceDescriptor;
    }
    
    public async Task<ProjectDataResult> GetProjectsForDevice(string deviceUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForDevice)} deviceUid: {deviceUid}");
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>($"/device/{deviceUid}/projects",
        deviceUid, null,
        customHeaders);

      log.LogDebug($"{nameof(GetProjectsForDevice)} get projects for deviceUid {deviceUid} result: {result.Code}, {result.Message}");
      return result;
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
