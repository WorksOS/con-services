using DbModel.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IDeviceTypeParameterAttributeRepository
	{
		Task<IEnumerable<DeviceTypeParameterAttributeDto>> Fetch(DeviceTypeParameterAttributeDto request);
	}
}
