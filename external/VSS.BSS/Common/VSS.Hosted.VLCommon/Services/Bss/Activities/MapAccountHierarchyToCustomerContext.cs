using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class MapAccountHierarchyToCustomerContext : IActivity
  {
    public const string NEW_SUMMERY_MESSAGE = @"Mapped message to CustomerContext.";
    public const string BSSID_FOUND_MESSAGE = @"{0} found for BSSID: {1}.";
    public const string BSSID_NOT_FOUND_MESSAGE = @"No customer found for BSSID: {0}.";

    public const string ADMINUSER_DEFINED_MESSAGE = @"PrimaryContact is defined in message.";
    public const string ADMINUSER_NOT_DEFINED_MESSAGE = @"No PrimaryContact defined in message.";

    public const string PARENTBSSID_DEFINED_MESSAGE = @"ParentBSSID is defined in message.";
    public const string PARENTBSSID_NOT_DEFINED_MESSAGE = @"No ParentBSSID defined in message.";
    public const string PARENTBSSID_FOUND_MESSAGE = @"{0} found for ParentBSSID: {1}.";
    public const string PARENTBSSID_NOT_FOUND_MESSAGE = @"No parent found for ParentBSSID: {0}.";

    private IList<string> _summary = new List<string>();

    public ActivityResult Execute(Inputs inputs)
    {
      var message = inputs.Get<AccountHierarchy>();
      var context = inputs.GetOrNew<CustomerContext>();

      MapMessageToCustomerContextNew(message, context);

      /*
		   * Get Current Customer using BSSID if customer is found
		   */
      MapMessageToCustomerContextCurrent(message, context);

      /*
		   * Map PrimaryContact to NewAdminUser
		   */
      //MapMessageToCustomerContextNewAdminUser(message, context);

      /*
		   * Map ParentBSSID to NewParent
		   */
      MapMessageToCustomerContextNewParent(message, context);

      return new ActivityResult {Summary = _summary.ToNewLineString()};
    }

    #region Mapping methods

    private void MapMessageToCustomerContextNew(AccountHierarchy message, CustomerContext context)
    {
      context.New.BssId = message.BSSID;
      context.New.Type = message.CustomerType.ToEnum<CustomerTypeEnum>();
      context.New.Name = message.CustomerName;
      context.New.DealerNetwork = message.DealerNetwork.ToDealerNetworkEnum();
      context.New.NetworkDealerCode = message.NetworkDealerCode;
      context.New.NetworkCustomerCode = message.NetworkCustomerCode;
      context.New.DealerAccountCode = message.DealerAccountCode;
      context.New.PrimaryEmailContact = message.contact.Email;
      context.New.FirstName = message.contact.FirstName;
      context.New.LastName = message.contact.LastName;

      _summary.Add(NEW_SUMMERY_MESSAGE);
      _summary.Add(context.New.PropertiesAndValues().ToNewLineTabbedString());
    }

    private void MapMessageToCustomerContextCurrent(AccountHierarchy message, CustomerContext context)
    {
      var customer = Services.Customers().GetCustomerByBssId(message.BSSID);

      if (customer == null)
      {
        _summary.Add(string.Format(BSSID_NOT_FOUND_MESSAGE, message.BSSID));
      }
      else
      {
        context.Id = customer.ID;
        context.IsActive = customer.IsActivated;
        context.MapCustomer(customer);
        _summary.Add(string.Format(BSSID_FOUND_MESSAGE, (CustomerTypeEnum)customer.fk_CustomerTypeID, message.BSSID));
        _summary.Add(string.Format("ID: {0}", context.Id));
        _summary.Add(string.Format("Type: {0}", context.Type));
        _summary.Add(string.Format("Name: {0}", context.Name));
        _summary.Add(string.Format("DealerNetwork: {0}", context.DealerNetwork));
        _summary.Add(string.Format("NetworkDealerCode: {0}", context.NetworkDealerCode));
        _summary.Add(string.Format("NetworkCustomerCode: {0}", context.NetworkCustomerCode));
        _summary.Add(string.Format("DealerAccountCode: {0}", context.DealerAccountCode));
        _summary.Add(string.Format("PrimaryEmailContact: {0}", context.PrimaryEmailContact));
        _summary.Add(string.Format("FirstName: {0}", context.FirstName));
        _summary.Add(string.Format("LastName: {0}", context.LastName));
      }
    }

    private void MapMessageToCustomerContextNewParent(AccountHierarchy message, CustomerContext context)
    {
      if (string.IsNullOrWhiteSpace(message.ParentBSSID) == true)
      {
        _summary.Add(PARENTBSSID_NOT_DEFINED_MESSAGE);
      }
      else
      {
        _summary.Add(PARENTBSSID_DEFINED_MESSAGE);
        context.NewParent.BssId = message.ParentBSSID;
        context.NewParent.RelationshipId = message.RelationshipID;
        context.NewParent.RelationshipType = message.HierarchyType.ToCustomerRelationshipTypeEnum();
        context.NewParent.Type = context.NewParent.RelationshipType == CustomerRelationshipTypeEnum.TCSDealer
                                   ? CustomerTypeEnum.Dealer
                                   : CustomerTypeEnum.Customer;

        var existingParent = Services.Customers().GetCustomerByBssId(message.ParentBSSID);

        if (existingParent == null)
        {
          _summary.Add(string.Format(PARENTBSSID_NOT_FOUND_MESSAGE, message.ParentBSSID));
        }
        else
        {
          context.NewParent.Id = existingParent.ID;
          context.NewParent.Name = existingParent.Name;
          context.NewParent.IsActive = existingParent.IsActivated;
          context.NewParent.Type = (CustomerTypeEnum)existingParent.fk_CustomerTypeID;
          context.NewParent.DealerNetwork = (DealerNetworkEnum)existingParent.fk_DealerNetworkID;
          context.NewParent.NetworkDealerCode = existingParent.NetworkDealerCode;
          context.NewParent.NetworkCustomerCode = existingParent.NetworkCustomerCode;
          context.NewParent.DealerAccountCode = existingParent.DealerAccountCode;
          context.NewParent.CustomerUId = existingParent.CustomerUID;
          context.NewParent.PrimaryEmailContact = existingParent.PrimaryEmailContact;
          context.NewParent.FirstName = existingParent.FirstName;
          context.NewParent.LastName = existingParent.LastName;

          _summary.Add(string.Format(PARENTBSSID_FOUND_MESSAGE, (CustomerTypeEnum)existingParent.fk_CustomerTypeID, message.ParentBSSID));
        }

        _summary.Add(context.NewParent.PropertiesAndValues().ToNewLineTabbedString());
      }
    }

    private void MapMessageToCustomerContextNewAdminUser(AccountHierarchy message, CustomerContext context)
    {
      if (message.contact == null)
      {
        _summary.Add(ADMINUSER_NOT_DEFINED_MESSAGE);
      }
      else
      {
        context.AdminUser.FirstName = message.contact.FirstName;
        context.AdminUser.LastName = message.contact.LastName;
        context.AdminUser.Email = message.contact.Email;

        _summary.Add(ADMINUSER_DEFINED_MESSAGE);
        _summary.Add(context.AdminUser.PropertiesAndValues().ToNewLineTabbedString());
      }
    }

    #endregion

  }
}