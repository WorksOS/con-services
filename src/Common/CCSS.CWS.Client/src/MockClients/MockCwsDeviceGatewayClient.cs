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
using VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus;
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
      var deviceLksResponseModel = new DeviceLKSResponseModel()
      {
        TRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
        lat = 89.3, lon = 189.1,
        assetType = CWSDeviceTypeEnum.EC520,
        assetSerialNumber = serialNumber,
        deviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
        lastReported = DateTime.UtcNow.AddDays(-1),
      };
      log.LogDebug($"{nameof(GetDeviceLKS)} Mock: deviceLKSResponseModel {JsonConvert.SerializeObject(deviceLksResponseModel)}");
      return Task.FromResult(deviceLksResponseModel);
    }

    public Task<List<DeviceLKSResponseModel>> GetDevicesLKSForProject(Guid projectUid, DateTime? earliestOfInterestUtc, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesLKSForProject)} Mock: projectUid {projectUid} earliestOfInterestUtc {earliestOfInterestUtc}");

      var serialNumber = "12456YU";
      var devices = new List<DeviceLKSResponseModel>()
        {
          new DeviceLKSResponseModel()
          {
            TRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
            lat = 89.3, lon = 189.1,
            assetType = CWSDeviceTypeEnum.EC520,
            assetSerialNumber = serialNumber,
            deviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
            lastReported = DateTime.UtcNow.AddDays(-1),
          }
      };

      log.LogDebug($"{nameof(GetDevicesLKSForProject)} Mock: deviceLKSListResponseModel {JsonConvert.SerializeObject(devices)}");
      return Task.FromResult(devices);
    }

    public async Task CreateDeviceLKS(string deviceName, CreateDeviceLKSRequestModel createDeviceLksRequestModel, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateDeviceLKS)}  Mock: deviceName {deviceName} createDeviceLksRequestModel {JsonConvert.SerializeObject(createDeviceLksRequestModel)}");
    }
  }
}
