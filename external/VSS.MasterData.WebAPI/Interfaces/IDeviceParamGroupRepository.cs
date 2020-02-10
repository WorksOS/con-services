using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IDeviceParamGroupRepository
    {
        Task<IEnumerable<DeviceParamGroupDto>> FetchDeviceTypeParamGroups(DeviceParamGroupDto request);
        Task<IEnumerable<DeviceParamGroupDto>> FetchDeviceParameterGroupById(DeviceParamGroupDto request);
        Task<IEnumerable<DeviceParamGroupDto>> FetchAllDeviceParameterGroups(DeviceParamGroupDto request);
    }
}
