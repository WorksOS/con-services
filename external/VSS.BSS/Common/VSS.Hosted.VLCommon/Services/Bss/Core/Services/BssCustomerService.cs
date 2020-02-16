using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssCustomerService : IBssCustomerService
  {     
    public const string USER_CREATED_BY = "System";

    #region Customer

    public Customer GetCustomerByBssId(string bssId)
    {
      return Data.Context.OP.Customer.SingleOrDefault(x => x.BSSID == bssId);
    }

    public Tuple<Customer, CustomerRelationship> GetParentDealerByChildCustomerId(long childCustomerId)
    {
      return GetParentByChildCustomerId(childCustomerId, CustomerTypeEnum.Dealer);
    }

    public Tuple<Customer, CustomerRelationship> GetParentCustomerByChildCustomerId(long childCustomerId)
    {
      return GetParentByChildCustomerId(childCustomerId, CustomerTypeEnum.Customer);
    }
    
    private Tuple<Customer, CustomerRelationship> GetParentByChildCustomerId(long childCustomerId, CustomerTypeEnum customerType)
    {
      var parentInfo = (from customer in Data.Context.OP.CustomerReadOnly
                        join relationship in Data.Context.OP.CustomerRelationshipReadOnly on customer.ID equals relationship.fk_ParentCustomerID
                              where customer.fk_CustomerTypeID == (int)customerType
                              && relationship.fk_ClientCustomerID == childCustomerId
                              select new { customer, relationship }).SingleOrDefault();

            if (parentInfo != null)
        return new Tuple<Customer, CustomerRelationship>(parentInfo.customer, parentInfo.relationship);
      
      return new Tuple<Customer, CustomerRelationship>(null, null);
    }

    public Customer CreateCustomer(CustomerContext context)
    {

      Require.IsNotNull(context, "CustomerContext");
      Require.IsNotNull(context.New, "CustomerContext.New");

      switch (context.New.Type)
      {
        case CustomerTypeEnum.Dealer:

          return API.Customer.CreateDealer(
            Data.Context.OP, 
            context.New.Name, 
            context.New.BssId, 
            context.New.NetworkDealerCode, 
            context.New.DealerNetwork,
            context.New.PrimaryEmailContact,
            context.New.FirstName,
            context.New.LastName);

        case CustomerTypeEnum.Customer:

          return API.Customer.CreateCustomer(
            Data.Context.OP, 
            context.New.Name, 
            context.New.BssId,
            context.New.PrimaryEmailContact,
            context.New.FirstName,
            context.New.LastName);

        case CustomerTypeEnum.Account:

          return API.Customer.CreateAccount(
            Data.Context.OP,
            context.New.Name,
            context.New.BssId,
            context.New.DealerAccountCode,
            context.New.NetworkCustomerCode);

        default:

          return null;
      }
    }

    public bool UpdateCustomer(long customerId, List<Param> modifiedFields)
    {
      return API.Customer.Update(Data.Context.OP, customerId, modifiedFields);
    }

    public bool ReactivateCustomer(long customerId)
    {
      return API.Customer.Activate(Data.Context.OP, customerId);
    }

    public bool DeactivateCustomer(long customerId)
    {
      return API.Customer.Deactivate(Data.Context.OP, customerId);
    }

    public bool CustomerRelationshipExistForCustomer(long customerId)
    {
      var result = (from r in Data.Context.OP.CustomerRelationshipReadOnly
                    where r.fk_ClientCustomerID == customerId 
                      || r.fk_ParentCustomerID == customerId
                    select 1).Count() != 0;

      return result;
    }

    public bool ActiveServiceViewsExistForCustomer(long customerId)
    {
      int endKeyDate = DateTime.UtcNow.KeyDate();
      var result = (from s in Data.Context.OP.ServiceViewReadOnly
                    where s.fk_CustomerID == customerId 
                      && s.EndKeyDate >= endKeyDate
                    select 1).Count() != 0;
      return result;
    }

    public bool DevicesExistForCustomer(long customerId)
    {
      var result = (from d in Data.Context.OP.DeviceReadOnly
                    join c in Data.Context.OP.CustomerReadOnly on d.OwnerBSSID equals c.BSSID
                    where c.ID == customerId
                    select 1).Count() != 0;
      return result;
    }

    public Customer GetAccountCreatedByStore(string dealerAccountCode, ParentDto parent, string bssIdPrefix)
    {
      if (parent.Type != CustomerTypeEnum.Dealer || !parent.IsActive)
        return null;

      var accounts =
        (from customer in Data.Context.OP.CustomerReadOnly
          join relationship in Data.Context.OP.CustomerRelationshipReadOnly
            on customer.ID equals relationship.fk_ClientCustomerID
          where    relationship.fk_ParentCustomerID == parent.Id
                && relationship.fk_CustomerRelationshipTypeID == (int) parent.RelationshipType
                && customer.IsActivated
                && customer.fk_CustomerTypeID == (int) CustomerTypeEnum.Account
                && customer.DealerAccountCode == dealerAccountCode
                && customer.BSSID.StartsWith(bssIdPrefix)
          select customer).ToList();

      if (accounts.Count > 1)
        throw new Exception(string.Format("Multiple accounts with DealerAccountCode {0} are associated to parent dealer with BssID {1}", dealerAccountCode, parent.BssId));

      return accounts.FirstOrDefault();
    }

    public Customer GetCustomerCreatedByStore(string networkCustomerCode, string bssIdPrefix)
    {
      return (from customer in Data.Context.OP.CustomerReadOnly
              where customer.IsActivated
                 && customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer 
                 && customer.NetworkCustomerCode == networkCustomerCode
                 && customer.BSSID.StartsWith(bssIdPrefix)
              select customer).SingleOrDefault();
    }

    public Customer GetDealerCreatedByStore(string networkDealerCode, string bssIdPrefix)
    {
      return (from customer in Data.Context.OP.CustomerReadOnly
              where customer.IsActivated
                 && customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer 
                 && customer.NetworkDealerCode == networkDealerCode
                 && customer.BSSID.StartsWith(bssIdPrefix)
              select customer).SingleOrDefault();
    }

    public void UpdateCustomerBssId(string oldBssId, string newBssId)
    {
      var customer = (from c in Data.Context.OP.Customer
                      where c.BSSID == oldBssId
                      select c).SingleOrDefault();

      if (customer == null)
        throw new InvalidOperationException("Failed to find customer");

      var modifiedProperties = new List<Param> {new Param {Name = "BSSID", Value = newBssId}};
      API.Update(Data.Context.OP, customer, modifiedProperties);
    }

    #endregion

    #region CustomerRelationship

    public CustomerRelationship GetRelationshipById(string bssCustomerRelationshipId)
    {
      return Data.Context.OP.CustomerRelationshipReadOnly
             .FirstOrDefault(x => x.BSSRelationshipID == bssCustomerRelationshipId);
    }

    public bool CreateCustomerRelationship(CustomerContext context)
    {
      Require.IsNotNull(context, "CustomerContext");
      Require.IsNotNull(context.NewParent, "CustomerContext.NewParent");

      bool success = API.Customer.CreateCustomerRelationship(
        Data.Context.OP,
        context.NewParent.Id,
        context.Id,
        context.NewParent.RelationshipId,
        context.NewParent.RelationshipType);

      return success;
    }

    public bool DeleteCustomerRelationship(long parentId, long childId)
    {
      return API.Customer.RemoveCustomerRelationship(Data.Context.OP, parentId, childId);
    }

    public bool UpdateCustomerRelationshipId(long parentId, long childId, string relationshipId)
    {
      return API.Customer.UpdateCustomerRelationshipId(Data.Context.OP, parentId, childId, relationshipId);
    }

    public CustomerRelationship GetCustomerRelationshipWithIdPrefix(CustomerRelationshipTypeEnum relationshipType,
      long parentId, long childId, string bssRelationshipIdPrefix)
    {
      return (from c in Data.Context.OP.CustomerRelationshipReadOnly
              where c.fk_ParentCustomerID == parentId
                 && c.fk_ClientCustomerID == childId
                 && c.fk_CustomerRelationshipTypeID == (int) relationshipType
                 && c.BSSRelationshipID.StartsWith(bssRelationshipIdPrefix)
              select c).SingleOrDefault();
    }

    #endregion

    #region Users

    public bool AdminUserExistsForCustomer(long customerId)
    {
      return Data.Context.OP.UserReadOnly.FirstOrDefault(x => x.fk_CustomerID == customerId && x.Active) != null;
    }

    public User CreateAdminUser(long customerId, string firstName, string lastName, string email)
    {
      string userName = API.User.GetUniqueUserName();
      string password = string.Empty;
      string timeZoneName = string.Empty;
      DateTime? passwordExpiryDate = null;
      int languageId = 0;
      int? units = null;
      byte? locationDisplayTypeId = null;
      string globalId = Guid.NewGuid().ToString();
      byte? assetLabelPreferenceType = 1;
      string jobTitle = null;
      string address = null;
      string phoneNumber = null;
      List<UserFeatureAccess> userFeatureAccesses = UserFeatureAccess.GetNHWebAdminFeatureAccess();
      int? meterLabelPreferenceType = 1;
      TemperatureUnitEnum temperatureUnit = TemperatureUnitEnum.Fahrenheit;
      PressureUnitEnum pressureUnit = PressureUnitEnum.PSI;
      
      User newUser = API.User.Create(Data.Context.OP, customerId, userName, password, timeZoneName, 
                                     email, passwordExpiryDate, languageId, units, 
                                     locationDisplayTypeId, globalId, assetLabelPreferenceType,
                                     firstName, lastName, jobTitle, address, phoneNumber, userFeatureAccesses, meterLabelPreferenceType,
                                           temperatureUnit, pressureUnit, createdBy: USER_CREATED_BY);
      return newUser;
    }

    public bool IsEmailIdUnique(long customerId, string email)
    {
      return !API.User.EmailIDExists(Data.Context.OP,email,string.Empty,customerId);
    }

    public User GetFirstAdminUser(long customerId)
    {
      return API.User.GetFirstAdminUser(customerId);
    }

    public void UpdateAdminUser(long userId, string firstName, string lastName, string email)
    {
      API.User.Update(Data.Context.OP, userId, firstName: firstName, lastName: lastName, email: email);
    }
    #endregion

    #region CustomerReference

    public void AddCustomerReference(IBssReference addBssReference, long storeId, string alias, string value, Guid uid)
    {
      addBssReference.AddCustomerReference(storeId, alias, value, uid);
    }

    public void UpdateCustomerReference(IBssReference updateBssReference, string alias, string value, Guid uid)
    {
      updateBssReference.UpdateCustomerReference(alias, value, uid);
    }

    public IList<AccountInfo> GetDealerAccounts(IBssReference updateBssReference, Guid uid)
    {
      return updateBssReference.GetDealerAccounts(uid);
    }

    public bool HasStore(long customerId, StoreEnum store = StoreEnum.CAT)
    {
      return (from s in Data.Context.OP.CustomerStoreReadOnly 
              where s.fk_CustomerID == customerId && s.fk_StoreID == (long)store 
              select 1).Any();
    }

    public void CreateStore(long customerId, StoreEnum store = StoreEnum.CAT)
    {
      CustomerStore customerStore = new CustomerStore { fk_CustomerID = customerId, fk_StoreID = (long)store };
      Data.Context.OP.CustomerStore.AddObject(customerStore);

            if (Data.Context.OP.SaveChanges() <= 0)
        throw new InvalidOperationException("Failed to save Customer Store");
    }


        #endregion

  }
}
