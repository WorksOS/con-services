using DbModel.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Cache.Interfaces
{
	public interface IParameterAttributeCache
	{
		Task Initialize();
		Task<IEnumerable<DeviceTypeParameterAttributeDto>> Get(string deviceType);
		Task<IEnumerable<DeviceTypeParameterAttributeDto>> Get(string deviceType, string parameter);
		Task<IEnumerable<DeviceTypeParameterAttributeDto>> Get(string deviceType, string parameterGroup, string parameter);
	}
}
