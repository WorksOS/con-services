using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.Hosted.VLCommon.Services.Bss;

namespace UnitTests.BSS_Tests
{
  public class BssCustomerServiceFake : IBssCustomerService
  {
    private readonly CustomerRelationship _relationshipToReturn;
    private readonly User _adminUserToReturn;
    private readonly Tuple<Customer, CustomerRelationship> _parentAndRelationship;
    private readonly Customer _customerToReturn;
    private readonly bool _booleanToReturn;
    private readonly bool _relationshipsExist;
    private readonly bool _serviceViewsExist;
    private readonly bool _devicesExist;

    public bool WasExecuted { get; set; }

    #region CTOR
    public BssCustomerServiceFake(Customer customerToReturn)
    {
      _customerToReturn = customerToReturn;
    }

    public BssCustomerServiceFake(User adminUserToReturn)
    {
      _adminUserToReturn = adminUserToReturn;
    }

    public BssCustomerServiceFake(CustomerRelationship relationshipToReturn)
    {
      _relationshipToReturn = relationshipToReturn;
    }

    public BssCustomerServiceFake(Tuple<Customer, CustomerRelationship> parentAndRelationship)
    {
      _parentAndRelationship = parentAndRelationship;
    }

    public BssCustomerServiceFake(bool booleanToReturn)
    {
      _booleanToReturn = booleanToReturn;
    }

    public BssCustomerServiceFake(bool relationshipsExist = false, bool serviceViewsExist = false, bool devicesExist = false)
    {
      _relationshipsExist = relationshipsExist;
      _serviceViewsExist = serviceViewsExist;
      _devicesExist = devicesExist;
    }

    #endregion

    public Customer GetCustomerByBssId(string bssId)
    {
      WasExecuted = true;
      return _customerToReturn;
    }

    public Tuple<Customer, CustomerRelationship> GetParentDealerByChildCustomerId(long childCustomerId)
    {
      WasExecuted = true;
      return _parentAndRelationship;
    }

    public Tuple<Customer, CustomerRelationship> GetParentCustomerByChildCustomerId(long childCustomerId)
    {
      WasExecuted = true;
      return _parentAndRelationship;
    }

    public bool DeleteCustomerRelationship(long parentId, long childId)
    {
      WasExecuted = true;
      return _booleanToReturn;
    }

    public bool AdminUserExistsForCustomer(long customerId)
    {
      WasExecuted = true;
      return _booleanToReturn;
    }

    public Customer CreateCustomer(CustomerContext context)
    {
      WasExecuted = true;
      return _customerToReturn;
    }

    public bool UpdateCustomer(long customerId, List<Param> modifiedFields)
    {
      WasExecuted = true;
      return _booleanToReturn;
    }

    public bool ReactivateCustomer(long customerId)
    {
      WasExecuted = true;
      return _booleanToReturn;
    }

    public bool DeactivateCustomer(long customerId)
    {
      WasExecuted = true;
      return _booleanToReturn;
    }

    public void UpdateCustomerBssId(string oldBssId, string newBssId)
    {
      throw new NotImplementedException();
    }

    public CustomerRelationship GetRelationshipById(string bssCustomerRelationshipId)
    {
      WasExecuted = true;
      return _relationshipToReturn;
    }

    public User CreateAdminUser(long customerId, string firstName, string lastName, string email)
    {
      WasExecuted = true;
      return _adminUserToReturn;
    }

    public bool CreateCustomerRelationship(CustomerContext context)
    {
      WasExecuted = true;
      return _booleanToReturn;
    }

    public bool CustomerRelationshipExistForCustomer(long customerId)
    {
      WasExecuted = true;
      return _relationshipsExist;
    }

    public bool ActiveServiceViewsExistForCustomer(long customerId)
    {
      WasExecuted = true;
      return _serviceViewsExist;
    }

    public bool DevicesExistForCustomer(long customerId)
    {
      WasExecuted = true;
      return _devicesExist;
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
      return null;
    }

    public void AddCustomerReference(IBssReference addBssReference, long storeId, string alias, string value, Guid uid)
    {
      WasExecuted = true;
    }

    public bool HasStore(long customerId, StoreEnum store = StoreEnum.CAT)
    {
      WasExecuted = true;
      if (store == StoreEnum.CAT)
        return false;

      return true;
    }

    public void CreateStore(long customerId, StoreEnum store = StoreEnum.CAT)
    {
      WasExecuted = true;
    }


    public void UpdateCustomerReference(IBssReference addBssReference, string alias, string value, Guid uid)
    {
      WasExecuted = true;
    }


    public IList<AccountInfo> GetDealerAccounts(IBssReference bssReference, Guid uid)
    {
      WasExecuted = true;
      return new List<AccountInfo> { new AccountInfo { DealerAccountCode = "15", CustomerUid = Guid.NewGuid() } };
    }


    public bool IsEmailIdUnique(long customerId, string email)
    {
      WasExecuted = true;
      return string.Compare(email, "unique") > 0;
    }

    public User GetFirstAdminUser(long customerId)
    {
      WasExecuted = true;
      return _adminUserToReturn;
    }

    public void UpdateAdminUser(long userId, string firstName, string lastName, string email)
    {
      WasExecuted = true;      
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