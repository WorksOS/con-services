using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerRelationshipUpdate : Activity
  {
    public const string CancelledMessage =
      @"CustomerRelationshipId update cancelled. Could not find appropriate relationship to delete.";

    public const string ReturnFalseMessage = @"Update of CustomerRelationshipId came back false for unknown reason.";

    public const string Message =
      @"CustomerRelationshipId {0} between Child {1} Id: {2} and Parent {3} Id: {4} updated to {5}";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();
      ParentDto parentToUpdate = null;

      if (context.NewParent.Type == CustomerTypeEnum.Dealer && context.ParentDealer.Exists)
        parentToUpdate = context.ParentDealer;

      if (context.NewParent.Type == CustomerTypeEnum.Customer && context.ParentCustomer.Exists)
        parentToUpdate = context.ParentCustomer;

      if (parentToUpdate == null)
        return Warning(CancelledMessage);

      try
      {
        var success = Services.Customers()
          .UpdateCustomerRelationshipId(parentToUpdate.Id, context.Id, context.NewParent.RelationshipId);

        if (!success)
          return Error(ReturnFalseMessage);

        if (context.ParentDealer.Equals(parentToUpdate))
          context.ParentDealer.RelationshipId = context.NewParent.RelationshipId;
        else if (context.ParentCustomer.Equals(parentToUpdate))
          context.ParentCustomer.RelationshipId = context.NewParent.RelationshipId;
      }
      catch (Exception ex)
      {
        return Exception(ex, "Failed to to update " + Message + ".", context.NewParent.RelationshipType, context.Type,
          context.Id, context.NewParent.Type, parentToUpdate.Id, context.NewParent.RelationshipId);
      }
      return Success("Successfully updated " + Message, context.NewParent.RelationshipType, context.Type, context.Id,
        context.NewParent.Type, parentToUpdate.Id, context.NewParent.RelationshipId);
    }
  }
}
