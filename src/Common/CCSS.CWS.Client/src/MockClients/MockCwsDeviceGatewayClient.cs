using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client.MockClients
{
  /// <summary>
  /// Mocks to use until we can get the real endpoints
  /// </summary>
  public class MockCwsDeviceGatewayClient : CwsDeviceGatewayManagerClient, ICwsDeviceGatewayClient
  {
    public MockCwsDeviceGatewayClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<DeviceLKSResponseModel> GetDeviceLKS(string deviceName, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceLKS)} Mock: serialNumber {deviceName}");

      var serialNumber = "12456YU";
      var  deviceLksResponseModel = new DeviceLKSResponseModel()
      {
        DeviceTRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
        Latitude = 89.3,
        Longitude = 189.1,
        DeviceType = CWSDeviceTypeEnum.EC520,
        SerialNumber = serialNumber,
        DeviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
        LastReportedUtc = DateTime.UtcNow.AddDays(-1),
      };
      log.LogDebug($"{nameof(GetDeviceLKS)} Mock: deviceLKSResponseModel {JsonConvert.SerializeObject(deviceLksResponseModel)}");
      return Task.FromResult(deviceLksResponseModel);
    }

    public Task<DeviceLKSListResponseModel> GetDevicesLKSForProject(Guid projectUid, DateTime? earliestOfInterestUtc, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesLKSForProject)} Mock: projectUid {projectUid} earliestOfInterestUtc {earliestOfInterestUtc}");

      var serialNumber = "12456YU";
      var deviceLKSListResponseModel = new DeviceLKSListResponseModel()
      {
        Devices = new List<DeviceLKSResponseModel>()
        {
          new DeviceLKSResponseModel()
          {
            DeviceTRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 89.3,
            Longitude = 189.1,
            DeviceType = CWSDeviceTypeEnum.EC520,
            SerialNumber = serialNumber,
            DeviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
            LastReportedUtc = DateTime.UtcNow.AddDays(-1),
          }
        }
      };

      log.LogDebug($"{nameof(GetDevicesLKSForProject)} Mock: deviceLKSListResponseModel {JsonConvert.SerializeObject(deviceLKSListResponseModel)}");
      return Task.FromResult(deviceLKSListResponseModel);
    }
  }
}
