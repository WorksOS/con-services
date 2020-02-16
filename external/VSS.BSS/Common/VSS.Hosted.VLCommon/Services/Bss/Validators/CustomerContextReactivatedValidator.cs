using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerContextReactivatedValidator : Validator<CustomerContext>
  {
    public override void Validate(CustomerContext context)
    {
      if (!context.Exists)
        AddError(BssFailureCode.CustomerDoesNotExist,
           string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, context.New.Type, context.New.BssId));

      if (context.IsActive)
        AddWarning(string.Format(BssConstants.Hierarchy.CUSTOMER_IS_ACTIVE, context.New.BssId));
    }
  }
}