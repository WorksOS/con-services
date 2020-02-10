using System;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IAccountService
	{
		DbAccount GetAccount(Guid accountUid);
		bool CreateAccount(CreateCustomerEvent createAccount);
		bool UpdateAccount(UpdateCustomerEvent updateAccount, DbAccount accountDetails);
		bool DeleteAccount(Guid customerAccountUid, DateTime actionUtc, DbAccount accountDetails);

		bool CreateAccountCustomerRelationShip(CreateCustomerRelationshipEvent customerRelationship, DbAccount account);
		bool CreateAccountCustomerRelationShip(Guid parentCustomerUID, Guid childCustomerUID,
			DbAccount account, DateTime actionUTC, string deleteType);
		bool DeleteAccountCustomerRelationShip(Guid parentCustomerUID, Guid childCustomerUID,
			DbAccount account, DateTime actionUTC);

	}
}
