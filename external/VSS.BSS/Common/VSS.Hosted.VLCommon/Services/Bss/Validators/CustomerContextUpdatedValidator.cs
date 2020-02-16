namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerContextUpdatedValidator : Validator<CustomerContext>
  {
    public override void Validate(CustomerContext context)
    {
      Require.IsNotNull(context, "CustomerContext");
      Require.IsNotNull(context.NewParent, "CustomerContext.NewParent");

      // Customer must exist
      if (!context.Exists)
        AddError(BssFailureCode.CustomerDoesNotExist, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, context.New.Type, context.New.BssId));

      if(!context.RelationshipIdExistsForCustomer() && context.RelationshipTypeExistsForCustomer())
        AddError(BssFailureCode.CustomerExists, string.Format(BssConstants.Hierarchy.RELATIONSHIP_TYPE_EXISTS, context.NewParent.Type));

      if (context.AdminUser != null && context.AdminUser.Email != null && !Services.Customers().IsEmailIdUnique(context.Id, context.AdminUser.Email))
        AddError(BssFailureCode.PrimaryContactInvalid, string.Format(BssConstants.Hierarchy.PRIMARY_CONTACT_EMAIL_DUPLICATE, context.AdminUser.Email));
    }
  }
}