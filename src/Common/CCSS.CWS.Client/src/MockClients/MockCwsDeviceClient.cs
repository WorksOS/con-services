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

namespace CCSS.CWS.Client.MockClients
{
  /// <summary>
  /// Mocks to use until we can get the real endpoints
  /// </summary>
  public class MockCwsDeviceClient : CwsProfileManagerClient, ICwsDeviceClient
  {
    public MockCwsDeviceClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceBySerialNumber)} Mock: serialNumber {serialNumber}");

      var deviceResponseModel = new DeviceResponseModel()
      {
        Id = Guid.NewGuid().ToString(),
        AccountId = Guid.NewGuid().ToString(),
        DeviceType = "EC520",
        DeviceName = "this is a device",
        Status = "Active",
        SerialNumber = serialNumber
      };
      log.LogDebug($"{nameof(GetDeviceBySerialNumber)} Mock: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return Task.FromResult(deviceResponseModel);
    }

    public Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceByDeviceUid)} Mock: deviceUid {deviceUid}");
      
      var deviceResponseModel = new DeviceResponseModel()
      {
        Id = deviceUid.ToString(),
        AccountId = Guid.NewGuid().ToString(),
        DeviceType = "EC520",
        DeviceName = "this is a device",
        Status = "Active",
        SerialNumber = "56556565"
      };

      log.LogDebug($"{nameof(GetDeviceByDeviceUid)} Mock: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return Task.FromResult(deviceResponseModel);
    }

    public Task<DeviceListResponseModel> GetDevicesForAccount(Guid accountUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesForAccount)} Mock: accountUid {accountUid}");

      var deviceListResponseModel = new DeviceListResponseModel()
      {
        Devices = new List<DeviceResponseModel>()
        {
          new DeviceResponseModel()
          {
            Id = Guid.NewGuid().ToString(),
            AccountId = accountUid.ToString(),
            DeviceType = "EC520",
            DeviceName = "this is a device",
            Status = "Active",
            SerialNumber = "56556565"
          }
        }
      };

      log.LogDebug($"{nameof(GetDevicesForAccount)} Mock: deviceListResponseModel {JsonConvert.SerializeObject(deviceListResponseModel)}");
      return Task.FromResult(deviceListResponseModel);
    }

    public Task<ProjectListResponseModel> GetProjectsForDevice(Guid deviceUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForDevice)} Mock: deviceUid {deviceUid}");

      var projectListResponseModel = new ProjectListResponseModel()
      {
        Projects = new List<ProjectResponseModel>()
        {
          new ProjectResponseModel()
          {
            accountId = Guid.NewGuid().ToString(),
            projectId = Guid.NewGuid().ToString(),
            projectName = "this is a project",
            timezone = "Timbucktoo",
            boundary = new ProjectBoundary()
            {
              type = "Polygon",
              coordinates = new List<double[,]>() { { new double[2, 2] { { 180, 90 }, { 180, 90 } } } }
            }
          }
        }
      };

      log.LogDebug($"{nameof(GetProjectsForDevice)} Mock: projectListResponseModel {JsonConvert.SerializeObject(projectListResponseModel)}");
      return Task.FromResult(projectListResponseModel);
    }

  }
}
