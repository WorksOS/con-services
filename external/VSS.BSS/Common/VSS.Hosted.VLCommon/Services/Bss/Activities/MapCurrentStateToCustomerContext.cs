using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class MapCurrentStateToCustomerContext : IActivity
  {
    public const string PARENT_FOUND = @"Parent{0} found.";
    public const string PARENT_NOT_FOUND = @"No Parent{0} found.";

    public const string ADMIN_USER_FOUND = @"AdminUser found.";
    public const string ADMIN_USER_NOT_FOUND = @"No AdminUser found.";

    private IList<string> _summary = new List<string>();

    public ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.GetOrNew<CustomerContext>();

      _summary.Add(string.Format("Mapped {0}'s currently related entities.", context.Type));

      MapParentDealerToCustomerContext(context);

      MapParentCustomerToCustomerContext(context);

      MapAdminUserToCustomerContext(context);

      return new ActivityResult {Summary = _summary.ToNewLineString()};
    }

    #region Mapping Methods

    private void MapParentDealerToCustomerContext(CustomerContext context)
    {
      var parentDealerData = Services.Customers().GetParentDealerByChildCustomerId(context.Id);
      var parentDealer = parentDealerData.Item1;
      var dealerRelationship = parentDealerData.Item2;

      if (parentDealer != null)
      {
        context.ParentDealer.Id = parentDealer.ID;
        context.ParentDealer.IsActive = parentDealer.IsActivated;
        context.ParentDealer.MapCustomer(parentDealer);
        context.ParentDealer.RelationshipId = dealerRelationship.BSSRelationshipID;
        context.ParentDealer.RelationshipType = VSS.Hosted.VLCommon.CustomerRelationshipTypeEnum.TCSDealer;
        context.ParentDealer.DealerNetwork = (DealerNetworkEnum)parentDealer.fk_DealerNetworkID;
        context.ParentDealer.CustomerUId = parentDealer.CustomerUID;
        context.ParentDealer.NetworkDealerCode = parentDealer.NetworkDealerCode;
        _summary.Add(string.Format(PARENT_FOUND, "Dealer"));
        _summary.Add(string.Format(context.ParentDealer.PropertiesAndValues().ToNewLineString()));
      }
      else
      {
        _summary.Add(string.Format(PARENT_NOT_FOUND, "Dealer"));
      }
    }

    private void MapParentCustomerToCustomerContext(CustomerContext context)
    {
      var parentCustomerData = Services.Customers().GetParentCustomerByChildCustomerId(context.Id);
      var parentCustomer = parentCustomerData.Item1;
      var customerRelationship = parentCustomerData.Item2;

      if (parentCustomer != null)
      {
        context.ParentCustomer.Id = parentCustomer.ID;
        context.ParentCustomer.IsActive = parentCustomer.IsActivated;
        context.ParentCustomer.MapCustomer(parentCustomer);
        context.ParentCustomer.RelationshipId = customerRelationship.BSSRelationshipID;
        context.ParentCustomer.RelationshipType = CustomerRelationshipTypeEnum.TCSCustomer;
        context.ParentCustomer.CustomerUId = parentCustomer.CustomerUID;
        _summary.Add(string.Format(PARENT_FOUND, "Customer"));
        _summary.Add(context.ParentCustomer.PropertiesAndValues().ToNewLineString());
      }
      else
      {
        _summary.Add(string.Format(PARENT_NOT_FOUND, "Customer"));
      }
    }

    private void MapAdminUserToCustomerContext(CustomerContext context)
    {
      context.AdminUserExists = Services.Customers().AdminUserExistsForCustomer(context.Id);
      if (context.AdminUserExists)
        _summary.Add(ADMIN_USER_FOUND);
      else
        _summary.Add(ADMIN_USER_NOT_FOUND);
    }

    #endregion
  }
}
