using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.Hosted.VLCommon.Services.Bss;

namespace UnitTests.BSS_Tests
{
  public class BssCustomerServiceExceptionFake : IBssCustomerService
  {
    public bool WasExecuted { get; set; }

    #region NOT IMPLEMENTED
    public Customer GetCustomerByBssId(string bssId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public Tuple<Customer, CustomerRelationship> GetParentDealerByChildCustomerId(long childCustomerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public Tuple<Customer, CustomerRelationship> GetParentCustomerByChildCustomerId(long childCustomerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool DeleteCustomerRelationship(long parentId, long childId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool AdminUserExistsForCustomer(long customerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public Customer CreateCustomer(CustomerContext context)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool UpdateCustomer(long customerId, List<Param> modifiedFields)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool ReactivateCustomer(long customerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool DeactivateCustomer(long customerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void UpdateCustomerBssId(string oldBssId, string newBssId)
    {
      throw new NotImplementedException();
    }

    public CustomerRelationship GetRelationshipById(string bssCustomerRelationshipId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public User CreateAdminUser(long customerId, string firstName, string lastName, string email)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool CreateCustomerRelationship(CustomerContext context)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool CustomerRelationshipExistForCustomer(long customerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool ActiveServiceViewsExistForCustomer(long customerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool DevicesExistForCustomer(long customerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public Customer GetAccountCreatedByStore(string dealerAccountCode, ParentDto parent, string bssIdPrefix)
    {
      throw new NotImplementedException();
    }

    public Customer GetCustomerCreatedByStore(string networkCustomerCode, string bssIdPrefix)
    {
      throw new NotImplementedException();
    }

    public Customer GetDealerCreatedByStore(string networkDealerCode, string bssIdPrefix)
    {
      throw new NotImplementedException();
    }

    #endregion


    public void AddCustomerReference(IBssReference addBssReference, long storeId, string alias, string value, Guid uid)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool HasStore(long customerId, StoreEnum store = StoreEnum.CAT)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void CreateStore(long customerId, StoreEnum store = StoreEnum.CAT)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }


    public void UpdateCustomerReference(IBssReference addBssReference, string alias, string value, Guid uid)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }


    public IList<AccountInfo> GetDealerAccounts(IBssReference addBssReference, Guid uid)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }


    public bool IsEmailIdUnique(long customerId, string email)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public User GetFirstAdminUser(long customerId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void UpdateAdminUser(long userId, string firstName, string lastName, string email)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }


    public bool UpdateCustomerRelationshipId(long parentId, long childId, string relationshipId)
    {
      throw new NotImplementedException();
    }


    public CustomerRelationship GetCustomerRelationshipWithIdPrefix(CustomerRelationshipTypeEnum relationshipType, long parentId, long childId, string bssRelationshipIdPrefix)
    {
      throw new NotImplementedException();
    }
  }
}