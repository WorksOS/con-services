using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsDeviceClient
  {
    Task<DeviceResponseModel> GetDeviceBySerialNumber(string serialNumber, IHeaderDictionary customHeaders = null);
    Task<DeviceResponseModel> GetDeviceByDeviceUid(Guid deviceUid, IHeaderDictionary customHeaders = null);
    Task<DeviceListResponseModel> GetDevicesForAccount(Guid accountUid, IHeaderDictionary customHeaders = null);
    Task<ProjectListResponseModel> GetProjectsForDevice(Guid deviceUid, IHeaderDictionary customHeaders = null);
    Task<DeviceAccountListResponseModel> GetAccountsForDevice(Guid deviceUid, IHeaderDictionary customHeaders = null);
  }
}
