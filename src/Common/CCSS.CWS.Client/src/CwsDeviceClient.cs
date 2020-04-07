using System;
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
  public class CwsDeviceClient : CwsProfileManagerClient, ICwsDeviceClient
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
      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/{serialNumber}", null, null, null, customHeaders);
      // todoMaveric what if error?
      deviceResponseModel.Id = TRNHelper.ExtractGuidAsString(deviceResponseModel.Id);
      deviceResponseModel.AccountId = TRNHelper.ExtractGuidAsString(deviceResponseModel.AccountId);

      return deviceResponseModel;
    }

    /// <summary>
    /// GET https://api-stg.trimble.com/t/trimble.com/cws-devicegateway-stg/2.0/devices/{deviceId}
    ///   application token
    ///   todoMaaverick used by TFA ProjectIDExecutor, projectBoundariesAtDateExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    ///   CCSSCON-114
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);

      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/{deviceTrn}", deviceUid, null, null, customHeaders);
      // todoMaveric what if error?
      deviceResponseModel.Id = TRNHelper.ExtractGuidAsString(deviceResponseModel.Id);
      deviceResponseModel.AccountId = TRNHelper.ExtractGuidAsString(deviceResponseModel.AccountId);
      return deviceResponseModel;
    }

    /// <summary>
    /// GET https://api.trimble.com/t/trimble.com/cws-devicegateway/1.0/accounts/{accountUid}/devices
    ///   application token
    ///   todoMaaverick used when UI calls ProjectSvc.GetCustomerDeviceLicense() 
    ///   to load devices for account into DB (to generate shortRaptorAssetId)
    ///                 response fields: DeviceTRN
    ///   CCSSCON-136
    /// </summary>
    public async Task<DeviceListResponseModel> GetDevicesForAccount(Guid accountUid, IDictionary<string, string> customHeaders = null)
    {
      var accountTrn = TRNHelper.MakeTRN(accountUid, TRNHelper.TRN_ACCOUNT);
      var deviceListResponseModel = await GetData<DeviceListResponseModel>($"/accounts/{accountTrn}/devices", accountUid, null, null, customHeaders);
      //  parameters: &includeTccRegistrationStatus=true
      // todoMaveric what if error?

      foreach (var device in deviceListResponseModel.Devices)
      {
        device.Id = TRNHelper.ExtractGuidAsString(device.Id);
        device.AccountId = TRNHelper.ExtractGuidAsString(device.AccountId);
      }
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
      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var projectListResponse = await GetData<ProjectListResponseModel>($"/device/{deviceTrn}/projects", deviceUid, null, null, customHeaders);
      foreach (var project in projectListResponse.Projects)
      {
        project.accountId = TRNHelper.ExtractGuidAsString(project.accountId);
        project.projectId = TRNHelper.ExtractGuidAsString(project.projectId);
      }
      return projectListResponse;
    }

  }
}
