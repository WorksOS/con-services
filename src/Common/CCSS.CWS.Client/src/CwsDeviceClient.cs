﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
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
    public CwsDeviceClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    { }

    /// <summary>
    ///  used by TFA AssetIdExecutor; ProjectAndAssetUidsExecutor;  ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceBySerialNumber)}: serialNumber {serialNumber}");

      // todo Upper-casing serial number is a temporary kludge until the Jira US (CCSSSCON-1055) is resolved
      var queryParameters = new List<KeyValuePair<string, string>>{
          new KeyValuePair<string, string>("serialNumber", serialNumber.ToUpper())};
      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/getDeviceWithSerialNumber", null, null, queryParameters, customHeaders);

      log.LogDebug($"{nameof(GetDeviceBySerialNumber)}: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    /// <summary>
    /// used by TFA ProjectIDExecutor, projectBoundariesAtDateExecutor
    ///                 response fields: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    /// </summary>
    public async Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceByDeviceUid)}: deviceUid {deviceUid}");

      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var deviceResponseModel = await GetData<DeviceResponseModel>($"/devices/{deviceTrn}", deviceUid, null, null, customHeaders);

      log.LogDebug($"{nameof(GetDeviceByDeviceUid)}: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    /// <summary>
    /// used by TFA: projectIdExecutor; ProjectBoundariesAtDateExec; ProjectAndAssetUidsExecutor; ProjectAndAssetUidsEarthWorksExecutor
    ///                 response fields: ProjectTRN
    /// </summary>
    public async Task<ProjectListResponseModel> GetProjectsForDevice(Guid deviceUid, bool includeProjectSettings = true, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForDevice)}: deviceUid {deviceUid}");

      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var queryParameters = new List<KeyValuePair<string, string>>();
      if (includeProjectSettings)
        queryParameters.Add(new KeyValuePair<string, string>("includeProjectSettings", "true"));
      var projectListResponseModel = await GetAllPagedData<ProjectListResponseModel, ProjectResponseModel>($"/devices/{deviceTrn}/projects", deviceUid, null, queryParameters, customHeaders);

      log.LogDebug($"{nameof(GetProjectsForDevice)}: projectListResponseModel {JsonConvert.SerializeObject(projectListResponseModel)}");
      return projectListResponseModel;
    }

    /// <summary>
    /// gets accounts related to a device
    ///    should only be 1 RelationStatus.Active
    /// this is a temp requirement until CCSSSCON-28
    /// </summary>
    public async Task<DeviceAccountListResponseModel> GetAccountsForDevice(Guid deviceUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetAccountsForDevice)}: deviceUid {deviceUid}");
      var deviceTrn = TRNHelper.MakeTRN(deviceUid, TRNHelper.TRN_DEVICE);
      var deviceAccountListResponseModel = await GetAllPagedData<DeviceAccountListResponseModel, DeviceAccountResponseModel>($"/devices/{deviceTrn}/accounts", deviceUid, null, null, customHeaders);

      log.LogDebug($"{nameof(GetAccountsForDevice)}: deviceAccountListResponseModel {JsonConvert.SerializeObject(deviceAccountListResponseModel)}");
      return deviceAccountListResponseModel;
    }
  }
}
