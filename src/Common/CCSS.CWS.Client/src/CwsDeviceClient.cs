using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
  public class CwsDeviceClient : CwsProfileManagerClient, ICwsDeviceClient
  {
    public CwsDeviceClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// GET https://api-stg.trimble.com/t/trimble.com/cws-profilemanager-stg/1.0/devices/getDeviceWithSerialNumber?serialNumber=CB123
    ///   application token
    ///   todoMaaverick used by TFA AssetIdExecutor; ProjectAndAssetUidsExecutor;  ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    ///   CCSSSCON-115
    ///        2020_04_17 todoWM the response has no AccountId , name or status yet?
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceBySerialNumber)}: serialNumber {serialNumber}");

      var queryParameters = new List<KeyValuePair<string, string>>{
          new KeyValuePair<string, string>("serialNumber", serialNumber)};
      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/getDeviceWithSerialNumber", null, null, queryParameters, customHeaders);
      // todoMaveric what if error?
      deviceResponseModel.Id = TRNHelper.ExtractGuidAsString(deviceResponseModel.Id);
      deviceResponseModel.AccountId = TRNHelper.ExtractGuidAsString(deviceResponseModel.AccountId);

      log.LogDebug($"{nameof(GetDeviceBySerialNumber)}: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    /// <summary>
    /// GET https://api-stg.trimble.com/t/trimble.com/cws-profilemanager-stg/1.0/devices/trn::profilex:us-west-2:device:08d7b6bb-c4d5-209a-01d9-bf00010008b5
    ///   application token
    ///   todoMaaverick used by TFA ProjectIDExecutor, projectBoundariesAtDateExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    ///   CCSSSCON-114
    ///        2020_04_17 todoWM the response has no AccountId or status yet?
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceByDeviceUid)}: deviceUid {deviceUid}");

      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/{deviceTrn}", deviceUid, null, null, customHeaders);
      // todoMaveric what if error?
      deviceResponseModel.Id = TRNHelper.ExtractGuidAsString(deviceResponseModel.Id);
      deviceResponseModel.AccountId = TRNHelper.ExtractGuidAsString(deviceResponseModel.AccountId);

      log.LogDebug($"{nameof(GetDeviceByDeviceUid)}: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    /// <summary>
    /// GET https://api-stg.trimble.com/t/trimble.com/cws-profilemanager-stg/1.0/accounts/trn::profilex:us-west-2:account:158ef953-4967-4af7-81cc-952d47cb6c6f%0A/devices?includeTccRegistrationStatus=true
    ///   application token
    ///   todoMaaverick used when UI calls ProjectSvc.GetCustomerDeviceLicense() 
    ///   to load devices for account into DB (to generate shortRaptorAssetId)
    ///                 response fields: DeviceTRN
    ///   CCSSSCON-136
    ///       2020_04_17 todoWM returns no devices yet?
    /// </summary>
    public async Task<DeviceListResponseModel> GetDevicesForAccount(Guid accountUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesForAccount)}: accountUid {accountUid}");

      var accountTrn = TRNHelper.MakeTRN(accountUid, TRNHelper.TRN_ACCOUNT);
      var deviceListResponseModel = await GetData<DeviceListResponseModel>($"/accounts/{accountTrn}/devices?includeTccRegistrationStatus=true", accountUid, null, null, customHeaders);
      // todoMaveric what if error?

      foreach (var device in deviceListResponseModel.Devices)
      {
        device.Id = TRNHelper.ExtractGuidAsString(device.Id);
        device.AccountId = TRNHelper.ExtractGuidAsString(device.AccountId);
      }

      log.LogDebug($"{nameof(GetDevicesForAccount)}: deviceListResponseModel {JsonConvert.SerializeObject(deviceListResponseModel)}");
      return deviceListResponseModel;
    }

    /// <summary>
    /// GET https://api.trimble.com/t/trimble.com/cws-device?? manager/1.0/projects/device/{deviceUid}
    ///   application token
    ///   todoMaaverick used by TFA: projectIdExecutor; ProjectBoundariesAtDateExec; ProjectAndAssetUidsExecutor; ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: ProjectTRN
    ///   CCSSCON-113
    /// </summary>
    public async Task<ProjectListResponseModel> GetProjectsForDevice(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForDevice)}: deviceUid {deviceUid}");

      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var projectListResponseModel = await GetData<ProjectListResponseModel>($"/device/{deviceTrn}/projects", deviceUid, null, null, customHeaders);
      foreach (var project in projectListResponseModel.Projects)
      {
        project.accountId = TRNHelper.ExtractGuidAsString(project.accountId);
        project.projectId = TRNHelper.ExtractGuidAsString(project.projectId);
      }

      log.LogDebug($"{nameof(GetProjectsForDevice)}: projectListResponseModel {JsonConvert.SerializeObject(projectListResponseModel)}");
      return projectListResponseModel;
    }

  }
}
