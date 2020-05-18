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
  ///  These use the cws-ProfileManager controller.
  /// NOTE: All calls require a TPaaS application token. This is because TFA doesn't have a user token.
  /// </summary>
  public class CwsDeviceClient : CwsProfileManagerClient, ICwsDeviceClient
  {
    // todoJeannie ProfileX throws exception on anything much over 20 (note that default is 10)
    private int FromRow = 0;
    private int RowCount = 20;

    public CwsDeviceClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    { }

    /// <summary>
    ///  used by TFA AssetIdExecutor; ProjectAndAssetUidsExecutor;  ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceBySerialNumber)}: serialNumber {serialNumber}");

      var queryParameters = new List<KeyValuePair<string, string>>{
          new KeyValuePair<string, string>("serialNumber", serialNumber)};
      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/getDeviceWithSerialNumber", null, null, queryParameters, customHeaders);
      deviceResponseModel.Id = TRNHelper.ExtractGuidAsString(deviceResponseModel.Id);

      log.LogDebug($"{nameof(GetDeviceBySerialNumber)}: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    /// <summary>
    /// used by TFA ProjectIDExecutor, projectBoundariesAtDateExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceByDeviceUid)}: deviceUid {deviceUid}");

      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/{deviceTrn}", deviceUid, null, null, customHeaders);
      deviceResponseModel.Id = TRNHelper.ExtractGuidAsString(deviceResponseModel.Id);

      log.LogDebug($"{nameof(GetDeviceByDeviceUid)}: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    /// <summary>
    /// 2020_05_05 this is probably obsolete now as devices will at best be done 1 at-a-time CCSSSCON-314
    /// used when UI calls ProjectSvc.GetCustomerDeviceLicense() 
    /// to load devices for account into DB (to generate shortRaptorAssetId)
    ///                 response fields: DeviceTRN
    /// </summary>
    public async Task<DeviceListResponseModel> GetDevicesForAccount(Guid accountUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesForAccount)}: accountUid {accountUid}");

      var accountTrn = TRNHelper.MakeTRN(accountUid, TRNHelper.TRN_ACCOUNT);
      var queryParameters = WithLimits(FromRow, RowCount);
      queryParameters.Add(new KeyValuePair<string, string>("includeTccRegistrationStatus", "true"));

      var deviceListResponseModel = await GetData<DeviceListResponseModel>($"/accounts/{accountTrn}/devices", accountUid, null, queryParameters, customHeaders);
      foreach (var device in deviceListResponseModel.Devices)
        device.Id = TRNHelper.ExtractGuidAsString(device.Id);

      log.LogDebug($"{nameof(GetDevicesForAccount)}: deviceListResponseModel {JsonConvert.SerializeObject(deviceListResponseModel)}");
      return deviceListResponseModel;
    }

    /// <summary>
    /// used by TFA: projectIdExecutor; ProjectBoundariesAtDateExec; ProjectAndAssetUidsExecutor; ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: ProjectTRN
    /// </summary>
    public async Task<ProjectListResponseModel> GetProjectsForDevice(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForDevice)}: deviceUid {deviceUid}");

      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var queryParameters = WithLimits(FromRow, RowCount);
      var projectListResponseModel = await GetData<ProjectListResponseModel>($"/device/{deviceTrn}/projects", deviceUid, null, queryParameters, customHeaders);
      foreach (var project in projectListResponseModel.Projects)
      {
        project.accountId = TRNHelper.ExtractGuidAsString(project.accountId);
        project.projectId = TRNHelper.ExtractGuidAsString(project.projectId);
      }

      log.LogDebug($"{nameof(GetProjectsForDevice)}: projectListResponseModel {JsonConvert.SerializeObject(projectListResponseModel)}");
      return projectListResponseModel;
    }

    /// <summary>
    /// gets accounts related to a device
    ///    should only be 1 RelationStatus.Active
    /// </summary>
    public async Task<DeviceAccountListResponseModel> GetAccountsForDevice(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetAccountsForDevice)}: deviceUid {deviceUid}");

      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var queryParameters = WithLimits(FromRow, RowCount);
      var deviceAccountListResponseModel = await GetData<DeviceAccountListResponseModel>($"/devices/{deviceTrn}/accounts", deviceUid, null, queryParameters, customHeaders);
      foreach (var account in deviceAccountListResponseModel.Accounts)
        account.Id = TRNHelper.ExtractGuidAsString(account.Id);

      log.LogDebug($"{nameof(GetAccountsForDevice)}: deviceAccountListResponseModel {JsonConvert.SerializeObject(deviceAccountListResponseModel)}");
      return deviceAccountListResponseModel;
    }

    private List<KeyValuePair<string, string>> WithLimits(int fromRow, int rowCount)
    {
      return new List<KeyValuePair<string, string>> 
        { new KeyValuePair<string, string>("from", fromRow.ToString()), 
          new KeyValuePair<string, string>("limit", rowCount.ToString())
        };
    }
  }
}
