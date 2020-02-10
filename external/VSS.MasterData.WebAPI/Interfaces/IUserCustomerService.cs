using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IUserCustomerService : ICustomerService
	{
		DbUserCustomer GetCustomerUser(Guid customerUid, Guid userUid);
		IEnumerable<DbUserCustomer> GetUsersForCustomer(Guid customerUid, List<Guid> userUids);
		bool AssociateCustomerUser(AssociateCustomerUserEvent associateCustomerUser);
		bool DissociateCustomerUser(DissociateCustomerUserEvent dissociateCustomerUser);
		bool BulkDissociateCustomerUser(Guid customerUid, List<Guid> userUids, DateTime actionUtc);
	}
}
