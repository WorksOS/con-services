using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsDeviceClient
  {
    Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IDictionary<string, string> customHeaders = null);
    Task<DeviceResponseModel> GetDeviceByDeviceUid(string deviceUid, IDictionary<string, string> customHeaders = null);
    Task<DeviceListResponseModel> GetDevicesForAccount(string accountUid, IDictionary<string, string> customHeaders = null);
    Task<ProjectListResponseModel> GetProjectsForDevice(string deviceUid, IDictionary<string, string> customHeaders = null);
  }
}
