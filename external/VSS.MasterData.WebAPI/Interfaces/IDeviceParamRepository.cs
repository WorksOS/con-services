using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IDeviceParamRepository
    {
        Task<IEnumerable<DeviceParamDto>> FetchDeviceTypeParameters(DeviceParamDto request);
        Task<IEnumerable<DeviceParamDto>> FetchDeviceTypeParametersByDeviceType(DeviceParamDto request);
    }
}
