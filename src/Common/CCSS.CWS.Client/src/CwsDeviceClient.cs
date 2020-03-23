using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  ///  These use the cws cws-devicegateway controller
  /// </summary>
  public class CwsDeviceClient : BaseClient, ICwsDeviceClient
  {
    public CwsDeviceClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient) : base(configuration, logger, gracefulClient)
    {
    }

    public async Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null)
    {
      //  https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/1332J023SW
      return await GetData<DeviceResponseModel>($"/devices/{serialNumber}"); 
    }

    public async Task<DeviceResponseModel> GetDeviceByDeviceUid(string deviceUid, IDictionary<string, string> customHeaders = null)
    {
      //  https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/trn::profilex:us-west-2:device:08d4c9ce-7b0e-d19c-c26a-a008a0000116
      return await GetData<DeviceResponseModel>($"/devices/{deviceUid}"); 
    }

    public async Task<DeviceListResponseModel> GetDevicesForAccount(string accountUid, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<DeviceListResponseModel>($"/accounts/{accountUid}/devices"); //  &includeTccRegistrationStatus=true", null, customHeaders);
    }

  }
}
