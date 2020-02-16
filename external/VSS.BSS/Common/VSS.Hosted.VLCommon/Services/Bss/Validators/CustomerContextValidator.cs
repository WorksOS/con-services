using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerContextValidator : Validator<CustomerContext>
  {
    public override void Validate(CustomerContext context)
    {
      Require.IsNotNull(context,"CustomerContext");
      Require.IsNotNull(context.New, "CustomerContext.New");
      Require.IsNotNull(context.NewParent, "CustomerContext.NewParent");

      if (context.NewParent.BssId.IsDefined() && 
         !context.NewParent.Exists)
        AddError(BssFailureCode.ParentDoesNotExist, 
          string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, context.NewParent.Type, context.NewParent.BssId));

      if (!context.ParentChildRelationshipIsValid())
        AddError(BssFailureCode.RelationshipInvalid, 
          string.Format(BssConstants.Hierarchy.RELATIONSHIP_INVALID, context.New.Type, context.NewParent.Type));

      if(context.NewParent.BssId.IsDefined() &&
         context.New.Type == CustomerTypeEnum.Customer &&
         context.NewParent.Type == CustomerTypeEnum.Customer)
        AddWarning(BssConstants.Hierarchy.CUSTOMER_WITH_PARENT_CUSTOMER);
    }
  }
}
