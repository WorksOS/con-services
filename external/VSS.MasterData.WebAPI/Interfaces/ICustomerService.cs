using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface ICustomerService
	{
		DbCustomer GetCustomer(Guid customerUid);
		List<DbCustomer> GetAssociatedCustomersbyUserUid(Guid userUid);
		List<DbCustomer> GetAssociatedCustomersForDealer(Guid customerUid);
		List<DbCustomer> GetCustomerByCustomerGuids(Guid[] customerUids);
		List<Tuple<DbCustomer, DbAccount>> GetCustomersByNameSearch(string filter, int maxResults);
		List<AssetCustomerDetail> GetAssetCustomerByAssetGuid(Guid assetUid);
		bool CreateCustomer(CreateCustomerEvent createCustomer);
		bool UpdateCustomer(UpdateCustomerEvent updateCustomer, DbCustomer customerDetails);
		bool DeleteCustomer(Guid customerUid, DateTime actionUtc);

		bool CreateUserCustomerRelationship(CreateUserCustomerRelationshipEvent userCustomerRelation);
		bool UpdateUserCustomerRelationship(UpdateUserCustomerRelationshipEvent userCustomerRelation);
		bool DeleteUserCustomerRelationship(DeleteUserCustomerRelationshipEvent userCustomerRelation);

		List<DbCustomerRelationshipNode> GetCustomerRelationships(Guid parentCustomerUid, Guid childCustomerUid);
		List<DbCustomerRelationshipNode> GetCustomerRelationshipsByCustomers(List<Guid> customerUids);
		bool CreateCustomerRelationShip(CreateCustomerRelationshipEvent customerRelationship);
		bool DeleteCustomerRelationShip(Guid parentCustomerUid, Guid childCustomerUid,
			Guid? accountUID, DateTime actionUTC);
		bool IsCustomerRelationShipAlreadyExists(string parentCustomerUID, string childCustomerUID);

		CustomerHierarchyInfo GetHierarchyInformationForUser(string targetUserUid,
			string targetCustomerUid = "", bool topLevelsOnly = false);
		List<Tuple<DbCustomer, DbCustomerRelationshipNode, DbAccount, string>> GetHierarchyForUser(Guid userUid,
			bool filterForHavingAssets, string targetCustomerUid = "");
		List<Tuple<DbCustomer, DbAccount>> GetOnlyAssociatedCustomersbyUserUid(Guid userUid);

		int GetAccountsCount(Guid dealerUid, Guid customerUid);
	}
}
