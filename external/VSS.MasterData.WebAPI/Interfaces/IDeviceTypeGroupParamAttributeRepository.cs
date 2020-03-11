using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IDeviceTypeGroupParamAttributeRepository
    {
        Task<IEnumerable<DeviceTypeGroupParamAttrDto>> Fetch(DeviceTypeGroupParamAttrDto request);
        Task<DeviceTypeParameterAttribute> Fetch(int deviceTypeID, string parameterName, string attributeName);
        Task<DeviceTypeParameterAttribute> Fetch(int DeviceTypeID, string ParameterName, string AttributeName, string deviceUID);
    }
}
