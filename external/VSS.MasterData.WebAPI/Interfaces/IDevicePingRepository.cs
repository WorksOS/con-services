using ClientModel.DeviceConfig.Request;
using DbModel.DeviceConfig;
using System;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IDevicePingRepository
    {
        Task<PingRequestStatus> Fetch(DevicePingLogRequest devicePingLogRequest);
        Task<PingRequestStatus> Insert(DevicePingLogRequest devicePingLogRequest);
        Task<DeviceTypeFamily> GetDeviceTypeFamily(Guid deviceUID);
    }
}
