using DbModel.DeviceConfig;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Cache.Interfaces
{
	public interface IDeviceTypeCache
	{
		Task Initialize();
		Task<IEnumerable<DeviceTypeDto>> Get(string deviceType);
	}
}
