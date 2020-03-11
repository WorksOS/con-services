using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.DbModel.Customer;
using VSS.MasterData.WebAPI.Transactions;

namespace CustomerRepository
{
	public class CustomerRepository : ICustomerRepository
	{
		private readonly ITransactions _transactions;

		public CustomerRepository(ITransactions transactions)
		{
			_transactions = transactions;
		}

		public async Task<IEnumerable<Customer>> GetCustomerInfo(IEnumerable<Guid> customerUids)
		{
			return await _transactions.GetAsync<Customer>(string.Format(
				"SELECT HEX(CustomerUID) as CustomerUID, CustomerName from md_customer_Customer where CustomerUID IN ({0})",
				string.Join(",", customerUids.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty)));
		}
	}
}