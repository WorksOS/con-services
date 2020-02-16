
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DealerNetworkUpdateValidator : Validator<CustomerContext>
  {
    public override void Validate(CustomerContext context)
    {
      // DealerNetwork may show up on all customer types, 
      // but VL logic should only use DealerNetwork on dealers.
      // So only validate if its a dealer.
      if(context.Type !=  CustomerTypeEnum.Dealer && context.New.Type != CustomerTypeEnum.Dealer)
        return;

      if (Services.Customers().DevicesExistForCustomer(context.Id))
      {
        AddError(BssFailureCode.CustomerTypeChangeInvalid, 
          BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, 
          context.DealerNetwork, 
          context.New.DealerNetwork, 
          "Device");
        return; // exit validator to reduce DB round trip
      }

      if (Services.Customers().ActiveServiceViewsExistForCustomer(context.Id))
      {
        AddError(BssFailureCode.CustomerTypeChangeInvalid, 
          BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, 
          context.DealerNetwork, 
          context.New.DealerNetwork, 
          "Active ServiceViews");
        return; // exit validator to reduce DB round trip
      }

      if (Services.Customers().CustomerRelationshipExistForCustomer(context.Id))
      {
        AddError(BssFailureCode.CustomerTypeChangeInvalid,
          BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, 
          context.DealerNetwork, 
          context.New.DealerNetwork, 
          "CustomerRelationship");
        return; // exit validator to reduce DB round trip
      }

    }
  }
}
