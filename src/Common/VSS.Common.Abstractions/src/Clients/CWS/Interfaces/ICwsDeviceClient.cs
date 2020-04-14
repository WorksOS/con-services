using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsDeviceClient
  {
    Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null);
    Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IDictionary<string, string> customHeaders = null);
    Task<DeviceListResponseModel> GetDevicesForAccount(Guid accountUid, IDictionary<string, string> customHeaders = null);
    Task<ProjectListResponseModel> GetProjectsForDevice(Guid deviceUid, IDictionary<string, string> customHeaders = null);
  }
}
