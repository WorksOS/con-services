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
  public class MockCwsDeviceClient : CwsProfileManagerClient, ICwsDeviceClient
  {
    public MockCwsDeviceClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceBySerialNumber)} Mock: serialNumber {serialNumber}");

      var deviceResponseModel = new DeviceResponseModel()
      {
        TRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_DEVICE),
        DeviceType = "EC520",
        DeviceName = "this is a device",
        SerialNumber = serialNumber
      };
      log.LogDebug($"{nameof(GetDeviceBySerialNumber)} Mock: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return Task.FromResult(deviceResponseModel);
    }

    public Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceByDeviceUid)} Mock: deviceUid {deviceUid}");
      
      var deviceResponseModel = new DeviceResponseModel()
      {
        TRN = TRNHelper.MakeTRN(deviceUid.ToString(),TRNHelper.TRN_DEVICE),
        DeviceType = "EC520",
        DeviceName = "this is a device",
        SerialNumber = "56556565"
      };

      log.LogDebug($"{nameof(GetDeviceByDeviceUid)} Mock: deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return Task.FromResult(deviceResponseModel);
    }

    public Task<DeviceListResponseModel> GetDevicesForAccount(Guid accountUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDevicesForAccount)} Mock: accountUid {accountUid}");

      var deviceListResponseModel = new DeviceListResponseModel()
      {
        Devices = new List<DeviceFromListResponseModel>()
        {
          new DeviceFromListResponseModel()
          {
            TRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(),TRNHelper.TRN_DEVICE),
            DeviceType = "EC520",
            DeviceName = "this is a device",
            SerialNumber = "56556565",
            RelationStatus = RelationStatusEnum.Active,
            TccDeviceStatus = TCCDeviceStatusEnum.Pending
          }
        }
      };

      log.LogDebug($"{nameof(GetDevicesForAccount)} Mock: deviceListResponseModel {JsonConvert.SerializeObject(deviceListResponseModel)}");
      return Task.FromResult(deviceListResponseModel);
    }

    public Task<ProjectListResponseModel> GetProjectsForDevice(Guid deviceUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForDevice)} Mock: deviceUid {deviceUid}");

      var projectListResponseModel = new ProjectListResponseModel()
      {
        Projects = new List<ProjectResponseModel>()
        {
          new ProjectResponseModel()
          {
            AccountTRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(),TRNHelper.TRN_ACCOUNT),
            ProjectTRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(),TRNHelper.TRN_PROJECT),
            ProjectName = "this is a project",
            Timezone = "Timbucktoo",
            Boundary = new ProjectBoundary()
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

    public Task<DeviceAccountListResponseModel> GetAccountsForDevice(Guid deviceUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetAccountsForDevice)} Mock: deviceUid {deviceUid}");

      var deviceAccountListResponseModel = new DeviceAccountListResponseModel()
      {
        Accounts = new List<DeviceAccountResponseModel>()
        {
          new DeviceAccountResponseModel()
          {
            TRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(),TRNHelper.TRN_DEVICE),
            AccountName = "an account name",
            RelationStatus = RelationStatusEnum.Active,
            TccDeviceStatus = TCCDeviceStatusEnum.Pending
          }
        }
      };

      log.LogDebug($"{nameof(GetAccountsForDevice)} Mock: deviceAccountListResponseModel {JsonConvert.SerializeObject(deviceAccountListResponseModel)}");
      return Task.FromResult(deviceAccountListResponseModel);
    }
  }
}
