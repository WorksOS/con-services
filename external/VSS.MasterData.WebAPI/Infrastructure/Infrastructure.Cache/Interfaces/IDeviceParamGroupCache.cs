using DbModel.DeviceConfig;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Cache.Interfaces
{
	public interface IDeviceParamGroupCache
	{
		Task Initialize();
		Task<IEnumerable<DeviceParamGroupDto>> Get(string groupName);
	}
}
