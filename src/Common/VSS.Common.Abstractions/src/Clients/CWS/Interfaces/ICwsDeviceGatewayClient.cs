using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsDeviceGatewayClient
  {
    Task<DeviceLKSResponseModel> GetDeviceLKS(string deviceName, IHeaderDictionary customHeaders = null);
    Task<List<DeviceLKSResponseModel>> GetDevicesLKSForProject(Guid projectUid, DateTime? earliestOfInterestUtc = null, IHeaderDictionary customHeaders = null);
    Task CreateDeviceLKS(string deviceName, DeviceLKSModel deviceLKSModel, IHeaderDictionary customHeaders = null);
  }
}
