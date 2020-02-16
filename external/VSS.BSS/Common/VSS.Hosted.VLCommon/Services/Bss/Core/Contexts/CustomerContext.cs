using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerDto
  {
    public CustomerTypeEnum Type { get; set; }
    public string BssId { get; set; }
    public string Name { get; set; }
    public DealerNetworkEnum DealerNetwork { get; set; }
    public string NetworkDealerCode { get; set; }
    public string NetworkCustomerCode { get; set; }
    public string DealerAccountCode { get; set; }
    public Guid? CustomerUId { get; set; }
    public string OldNetworkDealerCode { get; set; }
    public string OldNetworkCustomerCode { get; set; }
    public bool UpdatedNetworkCustomerCode { get; set; }
    public string OldDealerAccountCode { get; set; }
    public string PrimaryEmailContact { get; set; }
    public string OldPrimaryEmailContact { get; set; }
    public string FirstName { get; set; }
    public string OldFirstName { get; set; }
    public string LastName { get; set; }   
    public string OldLastName { get; set; }
  }

  public class ParentDto : CustomerDto
  {
    public bool Exists { get { return Id > 0; } }
    public long Id { get; set; }
    public bool IsActive { get; set; }
    public string RelationshipId { get; set; }
    public CustomerRelationshipTypeEnum RelationshipType { get; set; }
  }

  public class AdminUserDto
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
  }

  public class CustomerContext : CustomerDto
  {
    public CustomerContext()
    {
      ParentDealer = new ParentDto();
      ParentCustomer = new ParentDto();

      New = new CustomerDto();
      NewParent = new ParentDto();
      AdminUser = new AdminUserDto();
    }

    public bool Exists { get { return Id > 0; } }
    public long Id { get; set; }
    public bool IsActive { get; set; }
    public bool AdminUserExists { get; set; }

    public ParentDto ParentDealer { get; set; }
    public ParentDto ParentCustomer { get; set; }

    public CustomerDto New { get; set; }
    public ParentDto NewParent { get; set; }
    public AdminUserDto AdminUser { get; set; }
    public string CatStoreBssId { get; set; }

    private bool? _customerCreatedByCatStore;
    public bool CreatedByCatStore
    {
      get
      {
        if (!_customerCreatedByCatStore.HasValue)
          _customerCreatedByCatStore = CustomerCreatedByCatStore();
        return _customerCreatedByCatStore.Value;
      }
    }

    private bool? _customerRelationshipCreatedByCatStore;
    public bool RelationshipCreatedByCatStore
    {
      get
      {
        if (!_customerRelationshipCreatedByCatStore.HasValue)
          _customerRelationshipCreatedByCatStore = CustomerRelationshipCreatedByCatStore();
        return _customerRelationshipCreatedByCatStore.Value;
      }
    }

    private bool CustomerCreatedByCatStore()
    {
      var customer = GetCustomerWithBssIdPrefix(New, NewParent, "StoreAPI_");

      if (customer == null)
        return false;

      // We're hanging on to the CAT Store BssId so we can use itto update existing records with
      // the Trimble Store BssId
      CatStoreBssId = customer.BSSID;
      // We're using the Trimble Store BSSId; everything else will be CAT store data, which is
      // currently in the db
      BssId = New.BssId;               
      Id = customer.ID;
      Name = customer.Name;
      DealerNetwork = (DealerNetworkEnum)customer.fk_DealerNetworkID;
      NetworkDealerCode = customer.NetworkDealerCode;
      NetworkCustomerCode = customer.NetworkCustomerCode;
      DealerAccountCode = customer.DealerAccountCode;
      IsActive = customer.IsActivated;
      Type = (CustomerTypeEnum)customer.fk_CustomerTypeID;
      CustomerUId = customer.CustomerUID;

      // This would normally be done as part of the MapCurrentStateToCustomerContext activity if
      // the payload customer already existed from the perspective of Trimble Store.
      AdminUserExists = Services.Customers().AdminUserExistsForCustomer(customer.ID);

      // This would normally be done as part of the CustomerRelationshipCreate activity.  It must
      // done here because that activity won't be run on the assumption that CAT store has already
      // created the relationships.  We need this for the CustomerAddReference activity to work.
      switch (NewParent.Type)
      {
        case CustomerTypeEnum.Dealer:
          MapNewParentToExisting(NewParent, ParentDealer);
          break;
        case CustomerTypeEnum.Customer:
          MapNewParentToExisting(NewParent, ParentCustomer);
          break;
      }

      return true;
    }

    private bool CustomerRelationshipCreatedByCatStore()
    {
      ParentDto parent = null;

      if (NewParent.Type == CustomerTypeEnum.Dealer && ParentDealer.Exists)
        parent = ParentDealer;

      if (NewParent.Type == CustomerTypeEnum.Customer && ParentCustomer.Exists)
        parent = ParentCustomer;

      if (parent == null)
        return false;

      var customerRelationship = Services.Customers()
        .GetCustomerRelationshipWithIdPrefix(NewParent.RelationshipType, parent.Id, Id, "StoreAPI_");

      return customerRelationship != null;
    }

    private static void MapNewParentToExisting(ParentDto newParent, ParentDto existingParent)
    {
      existingParent.Id = newParent.Id;
      existingParent.BssId = newParent.BssId;
      existingParent.Name = newParent.Name;
      existingParent.DealerNetwork = newParent.DealerNetwork;
      existingParent.NetworkDealerCode = newParent.NetworkDealerCode;
      existingParent.NetworkCustomerCode = newParent.NetworkCustomerCode;
      existingParent.DealerAccountCode = newParent.DealerAccountCode;
      existingParent.RelationshipId = newParent.RelationshipId;
      existingParent.RelationshipType = newParent.RelationshipType;
      existingParent.IsActive = newParent.IsActive;
      existingParent.CustomerUId = newParent.CustomerUId;
    }

    private static Customer GetCustomerWithBssIdPrefix(CustomerDto customer, ParentDto parent, string bssIdPrefix)
    {
      switch (customer.Type)
      {
        case CustomerTypeEnum.Account:
          return Services.Customers().GetAccountCreatedByStore(customer.DealerAccountCode, parent, bssIdPrefix);
        case CustomerTypeEnum.Customer:
          return Services.Customers().GetCustomerCreatedByStore(customer.NetworkCustomerCode, bssIdPrefix);
        case CustomerTypeEnum.Dealer:
          return Services.Customers().GetDealerCreatedByStore(customer.NetworkDealerCode, bssIdPrefix);
        default:
          return null;
      }
    }

    public bool ParentChildRelationshipIsValid()
    {
      Require.IsNotNull(New, "CustomerContext.New");
      Require.IsNotNull(NewParent, "CustomerContext.NewParent");

      if (NewParent.BssId.IsNotDefined())
        return true;

      var child = New.Type;
      var parent = NewParent.Type;

      // Account can have a Dealer or a Customer as a parent
      if (child == CustomerTypeEnum.Account &&
          (parent == CustomerTypeEnum.Dealer ||
           parent == CustomerTypeEnum.Customer))
        return true;

      // Dealer can only have a Dealer as a parent
      if (child == CustomerTypeEnum.Dealer &&
          parent == CustomerTypeEnum.Dealer)
        return true;

      // Customer can only have a Customer as a parent
      // Warning
      if (child == CustomerTypeEnum.Customer &&
          parent == CustomerTypeEnum.Customer)
        return true;

      // All other relationship combinations are invalid
      return false;
    }

    public bool RelationshipTypeExistsForCustomer()
    {
      Require.IsNotNull(NewParent, "CustomerContext.NewParent");
      Require.IsNotNull(ParentDealer, "CustomerContext.ParentDealer");
      Require.IsNotNull(ParentCustomer, "CustomerContext.ParentCustomer");

      if (NewParent.BssId.IsNotDefined())
        return false;

      return (NewParent.Type == CustomerTypeEnum.Dealer && ParentDealer.Exists ||
              NewParent.Type == CustomerTypeEnum.Customer && ParentCustomer.Exists);
    }

    public bool RelationshipIdExistsForCustomer()
    {
      Require.IsNotNull(NewParent, "CustomerContext.NewParent");
      Require.IsNotNull(ParentDealer, "CustomerContext.ParentDealer");
      Require.IsNotNull(ParentCustomer, "CustomerContext.ParentCustomer");

      if (NewParent.RelationshipId.IsNotDefined())
        return false;

      if (ParentDealer.RelationshipId.IsDefined() &&
          ParentDealer.RelationshipId == NewParent.RelationshipId)
        return true;

      if (ParentCustomer.RelationshipId.IsDefined() &&
          ParentCustomer.RelationshipId == NewParent.RelationshipId)
        return true;

      return false;
    }

  }
}