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

    public async Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null)
    {
      //  https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/1332J023SW
      return await GetData<DeviceResponseModel>($"/devices/{serialNumber}", serialNumber, null, null, customHeaders);
    }

    public async Task<DeviceResponseModel> GetDeviceByDeviceUid(string deviceUid, IDictionary<string, string> customHeaders = null)
    {
      //  https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/trn::profilex:us-west-2:device:08d4c9ce-7b0e-d19c-c26a-a008a0000116
      return await GetData<DeviceResponseModel>($"/devices/{deviceUid}", deviceUid, null, null, customHeaders); 
    }

    public async Task<DeviceListResponseModel> GetDevicesForAccount(string accountUid, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<DeviceListResponseModel>($"/accounts/{accountUid}/devices", accountUid, null, null, customHeaders); 
      //  parameters: &includeTccRegistrationStatus=true
    }

  }
}
