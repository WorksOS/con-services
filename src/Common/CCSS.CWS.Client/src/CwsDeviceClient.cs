using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  ///  These use the cws cws-devicegateway controller
  /// </summary>
  public class CwsDeviceClient : BaseClient, ICwsDeviceClient
  {
    public CwsDeviceClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// GET https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/1332J023SW ??
    ///   application token
    ///   todoMaaverick used by TFA AssetIdExecutor; ProjectAndAssetUidsExecutor;  ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    ///   CCSSCON-115
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<DeviceResponseModel>($"/devices/{serialNumber}", null, null, null, customHeaders);
    }

    /// <summary>
    /// GET https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/{deviceId}
    ///   application token
    ///   todoMaaverick used by TFA ProjectIDExecutor, projectBoundariesAtDateExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    ///   CCSSCON-114
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceByDeviceUid(string deviceUid, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<DeviceResponseModel>($"/devices/{deviceUid}", deviceUid, null, null, customHeaders); 
    }

    /// <summary>
    /// GET https://api.trimble.com/t/trimble.com/cws-devicegateway/1.0/accounts/{accountUid}/devices
    ///   application token
    ///   todoMaaverick used when UI calls ProjectSvc.GetCustomerDeviceLicense() 
    ///   to load devices for account into DB (to generate shortRaptorAssetId)
    ///                 response fields: DeviceTRN
    ///   CCSSCON-136
    /// </summary>
    public async Task<DeviceListResponseModel> GetDevicesForAccount(string accountUid, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<DeviceListResponseModel>($"/accounts/{accountUid}/devices", accountUid, null, null, customHeaders); 
      //  parameters: &includeTccRegistrationStatus=true
    }

    /// <summary>
    /// GET https://api.trimble.com/t/trimble.com/cws-device?? manager/1.0/projects/device/{deviceUid}
    ///   application token
    ///   todoMaaverick used by TFA: projectIdExecutor; ProjectBoundariesAtDateExec; ProjectAndAssetUidsExecutor; ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: ProjectTRN
    ///   CCSSCON-113
    /// </summary>
    public async Task<ProjectListResponseModel> GetProjectsForDevice(string deviceUid, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<ProjectListResponseModel>($"/device/{deviceUid}/projects", deviceUid, null, null, customHeaders);
    }

  }
}
