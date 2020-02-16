using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerRelationshipDelete : Activity
  {
    public const string CANCELLED_MESSAGE = @"CustomerRelationship delete cancelled. Could not find appropriate relationship to delete.";
    public const string RETURN_FALSE_MESSAGE = @"Delete of CustomerRelationship came back false for unknown reason.";
    public const string MESSAGE = @"CustomerRelationship {0} between Child {1} Id: {2} Name: {3} and Parent {4} Id: {5} Name: {6}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();
      
      ParentDto parentToDelete = null;

      if (context.NewParent.Type == CustomerTypeEnum.Dealer && context.ParentDealer.Exists)
        parentToDelete = context.ParentDealer;

      if (context.NewParent.Type == CustomerTypeEnum.Customer && context.ParentCustomer.Exists)
        parentToDelete = context.ParentCustomer;

      if (parentToDelete == null)
        return Warning(CANCELLED_MESSAGE);

      try
      {
        bool success = Services.Customers().DeleteCustomerRelationship(parentToDelete.Id, context.Id);

        if (!success)
          return Error(RETURN_FALSE_MESSAGE);

        // Update context to reflect removal.
        if (context.ParentDealer.Equals(parentToDelete))
          context.ParentDealer = new ParentDto();
        else if (context.ParentCustomer.Equals(parentToDelete))
          context.ParentCustomer = new ParentDto();
      }
      catch (Exception ex)
      {
        return Exception(ex, "Failed to delete " + MESSAGE + ".",
          context.NewParent.RelationshipType,
          context.New.Type,
          context.Id,
          context.New.Name,
          context.NewParent.Type,
          parentToDelete.Id,
          parentToDelete.Name);
      }
      return Success("Deleted " + MESSAGE,
          context.NewParent.RelationshipType,
          context.Type,
          context.Id,
          context.Name,
          context.NewParent.Type,
          parentToDelete.Id,
          parentToDelete.Name);
    }
  }
}
