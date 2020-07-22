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
using VSS.Common.Abstractions.Enums;
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

      var serialNumberEC520 = "1234J56YU";
      var deviceLksResponseModel = new DeviceLKSResponseModel()
      {
        DeviceTrn = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
        Latitude = 89.3, Longitude = 179.1,
        AssetType = "Excavator",
        AssetSerialNumber = serialNumberEC520,
        DeviceName = $"{CWSDeviceTypeEnum.EC520}-{serialNumberEC520}",
        LastReportedUtc = DateTime.UtcNow.AddDays(-1),
      };
      log.LogDebug($"{nameof(GetDeviceLKS)} Mock: deviceLKSResponseModel {JsonConvert.SerializeObject(deviceLksResponseModel)}");
      return Task.FromResult(deviceLksResponseModel);
    }

    public Task<List<DeviceLKSResponseModel>> GetDevicesLKSForProject(Guid projectUid, DateTime? earliestOfInterestUtc, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesLKSForProject)} Mock: projectUid {projectUid} earliestOfInterestUtc {earliestOfInterestUtc}");

      var serialNumberEC520 = "1234J56YU";
      var serialNumberEC520W = "1234J06YU";
      var serialNumberTablet = Guid.NewGuid().ToString();
      var devices = new List<DeviceLKSResponseModel>()
        {
          new DeviceLKSResponseModel()
          {
            DeviceTrn = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 89.3, Longitude = 179.1,
            AssetType = "Excavator",
            AssetSerialNumber = serialNumberEC520,
            DeviceName = $"{CWSDeviceTypeEnum.EC520}-{serialNumberEC520}",
            LastReportedUtc = DateTime.UtcNow.AddDays(-1),
          },
          new DeviceLKSResponseModel()
          {
            DeviceTrn = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 22.5, Longitude = -150.21,
            AssetType = "Unknown",
            AssetSerialNumber = serialNumberEC520W,
            DeviceName = $"{CWSDeviceTypeEnum.EC520W.GetEnumMemberValue()}-{serialNumberEC520W}",
            LastReportedUtc = DateTime.UtcNow.AddDays(-2),
          },
          new DeviceLKSResponseModel()
          {
            DeviceTrn = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 16.5, Longitude = 168.21,
            AssetType = "Tablet",
            AssetSerialNumber = serialNumberTablet,
            DeviceName = $"{CWSDeviceTypeEnum.Tablet}-{serialNumberTablet}",
            LastReportedUtc = DateTime.UtcNow.AddDays(-1.5),
          }
      };

      log.LogDebug($"{nameof(GetDevicesLKSForProject)} Mock: deviceLKSListResponseModel {JsonConvert.SerializeObject(devices)}");
      return Task.FromResult(devices);
    }

    public Task CreateDeviceLKS(string deviceName, DeviceLKSModel deviceLKSModel, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateDeviceLKS)}  Mock: deviceName {deviceName} deviceLKSModel {JsonConvert.SerializeObject(deviceLKSModel)}");
      return Task.CompletedTask;
    }
  }
}
