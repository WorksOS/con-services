using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus;
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

      DeviceLKSResponseModel deviceLksResponseModel;
      try
      {
        deviceLksResponseModel = await GetData<DeviceLKSResponseModel>($"{ROUTE_PREFIX}/devicelks/{deviceName}", null, null, null, customHeaders);
      }
      catch (Exception e)
      {
        log.LogError(e, $"{nameof(GetDeviceLKS)}: failed to get deviceLKS. ");
        return null;
      }

      log.LogDebug($"{nameof(GetDeviceLKS)}: deviceLKSResponseModel {(deviceLksResponseModel == null ? null : JsonConvert.SerializeObject(deviceLksResponseModel))}");
      return deviceLksResponseModel;
    }

    /// <summary>
    /// Get devices LKS which last reported inside this project
    ///     Devices included are active and assigned to the project      
    ///     Optionally return only where device has reported after this earliestDate ("2020-04-29T07:02:57Z")
    /// As of 2020_07_06 pagination is not supported in cws for this endpoint.
    ///
    /// Note that the cws response is an unnamed list e.g. [{},{}] and not [devices:{},{}]
    ///    therefore the response must be a List<> and not a class containing a List<>
    ///    therefore this special need for GetDataNoCache()
    /// </summary>
    public async Task<List<DeviceLKSResponseModel>> GetDevicesLKSForProject(Guid projectUid, DateTime? earliestOfInterestUtc, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesLKSForProject)}: projectUID {projectUid}");
      var projectTrn = TRNHelper.MakeTRN(projectUid);
      var queryParameters = new List<KeyValuePair<string, string>>
        { new KeyValuePair<string, string>("projectId", projectTrn) };
      if (earliestOfInterestUtc != null)
        queryParameters.Add(new KeyValuePair<string, string>("lastReported", $"{earliestOfInterestUtc:yyyy-MM-ddTHH:mm:ssZ}"));
      var deviceLksListResponseModel = await GetDataNoCache<List<DeviceLKSResponseModel>>($"{ROUTE_PREFIX}/devicelks", queryParameters, customHeaders);

      log.LogDebug($"{nameof(GetDevicesLKSForProject)}: deviceLKSListResponseModel {(deviceLksListResponseModel == null ? null : JsonConvert.SerializeObject(deviceLksListResponseModel))}");
      return deviceLksListResponseModel;
    }

    /// <summary>
    /// Adding a location to cws using deviceName
    ///  CreateDeviceLocationRequestModel could be extended as other status supported
    /// </summary>
    public async Task CreateDeviceLKS(string deviceName, CreateDeviceLKSRequestModel createDeviceLksRequestModel, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateDeviceLKS)}: deviceName {deviceName} createDeviceLksRequestModel {JsonConvert.SerializeObject(createDeviceLksRequestModel)}");

      await PostData<CreateDeviceLKSRequestModel>($"{ROUTE_PREFIX}/status/{deviceName}", createDeviceLksRequestModel, null, customHeaders);
    }
  }
}
