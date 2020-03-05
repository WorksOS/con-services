using System;
using System.Threading.Tasks;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Response.DeviceConfig.Ping;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IDevicePingService
    {
        Task<DevicePingStatusResponse> GetPingRequestStatus(DevicePingLogRequest request);
        Task<DevicePingStatusResponse> PostDevicePingRequest(DevicePingLogRequest request);
    }
}
