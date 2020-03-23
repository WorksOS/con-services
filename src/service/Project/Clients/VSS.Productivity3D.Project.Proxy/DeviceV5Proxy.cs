﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Proxy
{
  public class DeviceV5Proxy : BaseServiceDiscoveryProxy, IDeviceProxy
  {
    public DeviceV5Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public  override bool IsInsideAuthBoundary => true;

    public  override ApiService InternalServiceType => ApiService.Device;

    public override string ExternalServiceName => null;

    public  override ApiVersion Version => ApiVersion.V5;

    public  override ApiType Type => ApiType.Public;

    public  override string CacheLifeKey => "DEVICE_CACHE_LIFE";

    public async Task<DeviceDataSingleResult> GetDevice(string serialNumber, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick at this stage we may not need to query also by device type. Question with Sankari re options
      // in ProjectSvc.DeviceV1Controller GetDevice
      // a) retrieve from cws using serialNumber which gets DeviceTRN etc
      //    https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/1332J023SW
      // b) get/create localDB shortRaptorAssetId so we can fill it into response

      // should do a cache by serialNumber
      log.LogDebug($"{nameof(GetDevice)} serialNumber: {serialNumber}");
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("serialNumber", serialNumber) };
      var result = await GetMasterDataItemServiceDiscoveryNoCache<DeviceDataSingleResult>($"/device/serialnumber",
        customHeaders, queryParams);

      if (result.Code == 0)
        return new DeviceDataSingleResult() { DeviceDescriptor = result.DeviceDescriptor };

      log.LogDebug($"Failed to get device with Uid {serialNumber} result: {result.Code}, {result.Message}");
      return null;
    }

    public async Task<DeviceDataSingleResult> GetDevice(int shortRaptorAssetId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick 
      // in ProjectSvc.DeviceController GetDevice
      // a) lookup localDB using shortRaptorAssetId to get DeviceTRN (return if not present)
      // b) retrieve from cws using DeviceTRN need licensed/not
      //    https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/trn::profilex:us-west-2:device:08d4c9ce-7b0e-d19c-c26a-a008a0000116

      // should do a cache by shortRaptorAssetId
      log.LogDebug($"{nameof(GetDevice)} shortRaptorAssetId: {shortRaptorAssetId}");
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("shortRaptorAssetId", shortRaptorAssetId.ToString()) };
      var result = await GetMasterDataItemServiceDiscoveryNoCache<DeviceDataSingleResult>($"/device/shortid",
        customHeaders, queryParams);

      if (result.Code == 0)
        return new DeviceDataSingleResult() { DeviceDescriptor = result.DeviceDescriptor };

      log.LogDebug($"Failed to get device with shortRaptorAssetId {shortRaptorAssetId} result: {result.Code}, {result.Message}");
      return null;
    }
    
    public async Task<ProjectDataResult> GetProjects(string deviceUid, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick 
      // in ProjectSvc.DeviceController
      // a) retrieve list of associated projects from cws using DeviceTRN.
      //    Response must include list of projects; device status for each licensed/pending/?

      // should do a cache by deviceUid
      log.LogDebug($"{nameof(GetProjects)} deviceUid: {deviceUid}");
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("deviceUid", deviceUid) };
      var result = await GetMasterDataItemServiceDiscoveryNoCache<ProjectDataResult>($"/project/applicationcontext/device",
        customHeaders, queryParams);

      if (result.Code == 0)
        return result;

      log.LogDebug($"Failed to get project for deviceUid {deviceUid} result: {result.Code}, {result.Message}");
      return null;
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (deviceUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId=null)
    {
      ClearCacheByTag(uid);

      if(string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }
    
  }
}
