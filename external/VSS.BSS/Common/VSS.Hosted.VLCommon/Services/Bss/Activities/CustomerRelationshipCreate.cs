using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerRelationshipCreate : Activity
  {

    public const string MESSAGE =  @"CustomerRelationship {0} between Child {1} Id: {2} Name: {3} and Parent {4} Id: {5} Name: {6}.";
    public const string RETURN_FALSE_MESSAGE = @"Creation of customer relationship failed for unknown reason.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();

      string message = string.Format(MESSAGE, 
        context.NewParent.RelationshipType, 
        context.New.Type, 
        context.Id,
        context.New.Name, 
        context.NewParent.Type, 
        context.NewParent.Id,
        context.NewParent.Name);

      try
      {
        bool success = Services.Customers().CreateCustomerRelationship(context);

        if (!success)
          return Error(RETURN_FALSE_MESSAGE);

        if(context.NewParent.Type == CustomerTypeEnum.Dealer)
        {
          MapNewParentToExisting(context.NewParent, context.ParentDealer);
        }
        else if(context.NewParent.Type == CustomerTypeEnum.Customer)
        {
          MapNewParentToExisting(context.NewParent, context.ParentCustomer);
        }
      }
      catch (Exception ex)
      {
        return Exception(ex, "Failed to create {0}.", message);
      }

      return Success("Created {0}", message);
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
  }
}