using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerContextDeletedValidator : Validator<CustomerContext>
  {
    public override void Validate(CustomerContext context)
    {
      Require.IsNotNull(context, "CustomerContext");
      Require.IsNotNull(context.New, "CustomerContext.New");
      Require.IsNotNull(context.NewParent, "CustomerContext.NewParent");

      if (!context.NewParent.Exists)
        AddError(BssFailureCode.ParentDoesNotExist, 
          string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, context.New.Type, context.New.BssId));

      if(!context.RelationshipIdExistsForCustomer())
        AddError(BssFailureCode.RelationshipIdDoesNotExist, 
          string.Format(BssConstants.Hierarchy.RELATIONSHIPID_DOES_NOT_EXIST, context.NewParent.RelationshipId));
    }
  }
}
