using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

    public Task<DeviceData> GetDevice(string serialNumber)
    {
      // todoMaverick at this stage we may not need device type. Question with Sankari re options
      // in ProjectSvc.DeviceController GetDevice
      // a) retrieve from cws using serialNumber which gets DeviceTRN etc
      //    https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/1332J023SW
      // b) get/create localDB shortRaptorAssetId so we can fill it into response
      throw new System.NotImplementedException();
    }

    public Task<DeviceData> GetDevice(long shortRaptorAssetId)
    {
      // todoMaverick 
      // in ProjectSvc.DeviceController GetDevice
      // a) lookup localDB using shortRaptorAssetId to get DeviceTRN (return if not present)
      // b) retrieve from cws using DeviceTRN need licensed/not
      //    https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/trn::profilex:us-west-2:device:08d4c9ce-7b0e-d19c-c26a-a008a0000116
      throw new System.NotImplementedException();
    }

    public Task<List<ProjectData>> GetProjects(string deviceUid)
    {
      // todoMaverick 
      // in ProjectSvc.DeviceController
      // a) retrieve list of associated projects from cws using DeviceTRN.
      //    Response must include list of projects; device status for each licensed/pending/?
      throw new System.NotImplementedException();
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
