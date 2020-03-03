using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IDeviceConfigMessageRepository
    {
        Task<DeviceConfigMessageDto> Fetch(string messageUID);
        Task<bool> Insert(DeviceConfigMessageDto deviceConfigMessageDto);
        Task<bool> Update(string messageUID);
    }
}
