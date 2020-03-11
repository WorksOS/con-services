using System.Collections.Generic;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IDeviceTypeRepository
    {
        Task<IEnumerable<DeviceTypeDto>> FetchDeviceTypes(DeviceTypeDto deviceTypeDto);
        Task<IEnumerable<DeviceTypeDto>> FetchAllDeviceTypes(DeviceTypeDto deviceTypeDto);
    }
}
