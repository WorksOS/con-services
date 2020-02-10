using System.Collections.Generic;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IDevicePingAckMessageRepository
    {
        Task<DevicePingACKMessageDto> Fetch(string messageUID);
        Task<bool> Update(DevicePingACKMessageDto devicePingACKMessageDto);
        Task<bool> UpdateRequestStatusInDevicePingLog(DevicePingACKMessageDto devicePingACKMessageDto);
        Task<IEnumerable<PingRequestStatus>> FetchDevicePingLogs(DevicePingACKMessageDto devicePingACKMessageDto);
    }
}
