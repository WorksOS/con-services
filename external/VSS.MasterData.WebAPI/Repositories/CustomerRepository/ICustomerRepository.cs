using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.DbModel.Customer;

namespace CustomerRepository
{
	public interface ICustomerRepository
	{
		Task<IEnumerable<Customer>> GetCustomerInfo(IEnumerable<Guid> customerUids);
	}
}