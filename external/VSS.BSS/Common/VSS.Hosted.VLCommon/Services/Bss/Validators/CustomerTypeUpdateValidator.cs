
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerTypeUpdateValidator : Validator<CustomerContext>
  {
    public override void Validate(CustomerContext context)
    {
      // We "Return ASAP" to avoid extra DB calls.

      // No matter what Type to Type change we are doing
      // it cannot have CustomerRelationships
      if (Services.Customers().CustomerRelationshipExistForCustomer(context.Id))
      {
        AddError(BssFailureCode.CustomerTypeChangeInvalid, 
          BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, 
          context.Type, 
          context.New.Type, 
          "CustomerRelationship");
        return;
      }

      // For Dealer or Customer Type changes
      // it cannot have active ServiceViews
      if((context.Type == CustomerTypeEnum.Dealer || context.Type == CustomerTypeEnum.Customer) 
        && Services.Customers().ActiveServiceViewsExistForCustomer(context.Id))
      {
        AddError(BssFailureCode.CustomerTypeChangeInvalid, 
          BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, 
          context.Type, 
          context.New.Type, 
          "Active ServiceViews");
        return;
      }

      // For Dealer or Account Type changes
      // it cannot have Devices
      if ((context.Type == CustomerTypeEnum.Dealer || context.Type == CustomerTypeEnum.Account) 
        && Services.Customers().DevicesExistForCustomer(context.Id))
      {
        AddError(BssFailureCode.CustomerTypeChangeInvalid,
          BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, 
          context.Type, 
          context.New.Type, 
          "Device");
        return;
      }
    }
  }
}