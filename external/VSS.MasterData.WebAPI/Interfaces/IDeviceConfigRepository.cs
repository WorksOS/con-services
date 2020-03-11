using DbModel.DeviceConfig;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IDeviceConfigRepository
    {
		Task<IEnumerable<DeviceConfigDto>> Fetch(IList<string> deviceUIDs, IList<DeviceConfigDto> deviceConfigDtos);
		Task<DeviceConfigDto> Insert(DeviceConfigDto deviceConfigDto);
		Task<IEnumerable<DeviceConfigDto>> Insert(IList<DeviceConfigDto> deviceConfigDtos);
		Task<DeviceConfigDto> Update(DeviceConfigDto deviceConfigDto);
		Task<bool> UpdateCurrentValue(DeviceConfigDto deviceConfigDto);
		Task<bool> Upsert(DeviceConfigDto deviceConfigDto);
		Task<IEnumerable<DeviceConfigDto>> Update(IList<DeviceConfigDto> deviceConfigDtos);
		Task<IEnumerable<DeviceConfigDto>> FetchDeviceConfigByParameterNames(List<string> parameterNames, bool getMaintenanceModeList = false);
	}
}
