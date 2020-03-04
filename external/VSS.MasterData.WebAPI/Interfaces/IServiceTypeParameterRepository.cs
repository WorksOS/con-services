using DbModel.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IServiceTypeParameterRepository
	{
		Task<IEnumerable<ServiceTypeParameterDto>> FetchAllServiceTypeParameter();
	}
}
