using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssCustomerService
  {
    // Customers
    Customer GetCustomerByBssId(string bssId);
    Tuple<Customer, CustomerRelationship> GetParentDealerByChildCustomerId(long childCustomerId);
    Tuple<Customer, CustomerRelationship> GetParentCustomerByChildCustomerId(long childCustomerId);
    Customer CreateCustomer(CustomerContext context);
    bool UpdateCustomer(long customerId, List<Param> modifiedFields);
    bool ReactivateCustomer(long customerId);
    bool DeactivateCustomer(long customerId);
    bool CustomerRelationshipExistForCustomer(long customerId);
    bool ActiveServiceViewsExistForCustomer(long customerId);
    bool DevicesExistForCustomer(long customerId);
    Customer GetAccountCreatedByStore(string dealerAccountCode, ParentDto parent, string bssIdPrefix);
    Customer GetCustomerCreatedByStore(string networkCustomerCode, string bssIdPrefix);
    Customer GetDealerCreatedByStore(string networkDealerCode, string bssIdPrefix);
    void UpdateCustomerBssId(string oldBssId, string newBssId);

    // CustomerRelationships
    CustomerRelationship GetRelationshipById(string bssCustomerRelationshipId);
    bool CreateCustomerRelationship(CustomerContext context);
    bool DeleteCustomerRelationship(long parentId, long childId);
    bool UpdateCustomerRelationshipId(long parentId, long childId, string relationshipId);
    CustomerRelationship GetCustomerRelationshipWithIdPrefix(CustomerRelationshipTypeEnum relationshipType,
      long parentId, long childId, string bssRelationshipIdPrefix);

    // Users
    bool AdminUserExistsForCustomer(long customerId);
    User CreateAdminUser(long customerId, string firstName, string lastName, string email);
    bool IsEmailIdUnique(long customerId, string email);
    User GetFirstAdminUser(long customerId);
    void UpdateAdminUser(long userId, string firstName, string lastName, string email);

    //CustomerReference
    void AddCustomerReference(IBssReference addBssReference, long storeId, string alias, string value, Guid uid);
    void UpdateCustomerReference(IBssReference addBssReference, string alias, string value, Guid uid);
    IList<AccountInfo> GetDealerAccounts(IBssReference addBssReference, Guid uid);
    bool HasStore(long customerId, StoreEnum store = StoreEnum.CAT);
    void CreateStore(long customerId, StoreEnum store = StoreEnum.CAT);
  }
}