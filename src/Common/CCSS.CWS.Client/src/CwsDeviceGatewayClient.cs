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
  ///  These use the cws-DeviceGateway controller.
  ///    This data is status and event-based e.g. get LastKnownStatus (LKS); post a new position
  /// </summary>
  public class CwsDeviceGatewayClient : CwsDeviceGatewayManagerClient, ICwsDeviceGatewayClient
  {
    public CwsDeviceGatewayClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    { }

    /// <summary>
    /// Get device LKS
    ///    Note that there is no restriction by last reported date or project
    ///    As of 2020_07_06 there is no way to query by deviceUid
    /// </summary>
    public async Task<DeviceLKSResponseModel> GetDeviceLKS(string deviceName, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceLKS)}: deviceName {deviceName}");

      var deviceLksResponseModel = await GetData<DeviceLKSResponseModel>($"/devicelks/{deviceName}", null, null, null, customHeaders);

      log.LogDebug($"{nameof(GetDeviceLKS)}: deviceLKSResponseModel {JsonConvert.SerializeObject(deviceLksResponseModel)}");
      return deviceLksResponseModel;
    }

    /// <summary>
    /// Get devices LKS which last reported inside this project
    ///     Devices included are active and assigned to the project      
    ///     Optionally return only where device has reported after this earliestDate ("2020-04-29T07:02:57Z")
    /// todoJeannie As of 2020_07_06 does this support paging?
    /// </summary>
    public async Task<DeviceLKSListResponseModel> GetDevicesLKSForProject(Guid projectUid, DateTime? earliestOfInterestUtc, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesLKSForProject)}: projectUID {projectUid}");
      var projectTrn = TRNHelper.MakeTRN(projectUid);
      var queryParameters = new List<KeyValuePair<string, string>>
        { new KeyValuePair<string, string>("projectid", projectTrn) };
      if (earliestOfInterestUtc != null)
        queryParameters.Add(new KeyValuePair<string, string>("lastReported", $"{earliestOfInterestUtc:yyyy-MM-ddTHH:mm:ssZ}"));
      var deviceLksListResponseModel = await GetData<DeviceLKSListResponseModel>($"/devicelks", projectUid, null, queryParameters, customHeaders);

      log.LogDebug($"{nameof(GetDevicesLKSForProject)}: deviceLKSListResponseModel {JsonConvert.SerializeObject(deviceLksListResponseModel)}");
      return deviceLksListResponseModel;
    }
  }
}