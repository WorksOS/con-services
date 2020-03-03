using DbModel.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Cache.Interfaces
{
	public interface IServiceTypeParameterCache
	{
		Task Initialize();
		Task<ServiceTypeParameterDto> Get(string deviceParameterName, string serviceTypeName);
		Task<IEnumerable<ServiceTypeParameterDto>> Get(string deviceParameterName);
	}
}
