namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerContextCreatedValidator : Validator<CustomerContext>
  {
    public override void Validate(CustomerContext context)
    {
      Require.IsNotNull(context, "CustomerContext");
      Require.IsNotNull(context.NewParent, "CustomerContext.NewParent");

      if(context.Exists)
        AddError(BssFailureCode.CustomerExists, string.Format(BssConstants.Hierarchy.BSSID_EXISTS, context.New.Type, context.New.BssId));

      if(context.RelationshipTypeExistsForCustomer())
        AddError(BssFailureCode.CustomerExists, string.Format(BssConstants.Hierarchy.RELATIONSHIP_TYPE_EXISTS, context.NewParent.Type));

      if(context.NewParent.RelationshipId.IsDefined() &&
         Services.Customers().GetRelationshipById(context.NewParent.RelationshipId) != null)
      {
        AddError(BssFailureCode.RelationshipIdExists, string.Format(BssConstants.Hierarchy.RELATIONSHIPID_EXISTS, context.NewParent.RelationshipId));
      }
    }
  }
}