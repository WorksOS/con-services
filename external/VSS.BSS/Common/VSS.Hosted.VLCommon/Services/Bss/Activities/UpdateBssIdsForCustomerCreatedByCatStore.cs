using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class UpdateBssIdsForCustomerCreatedByCatStore : Activity
  {
    public const string SuccessMessage = @"Updated CAT Store BSS Id {0} with Trimble Store BSS Id {1}.";
    public const string FailureMessage = @"Failed to update CAT Store BSS Id {0} with Trimble Store BSS Id {1}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();

      try
      {
        Services.Devices().UpdateDeviceOwnerBssIds(context.CatStoreBssId, context.BssId);
        Services.Assets().UpdateAssetDeviceHistoryBssIds(context.CatStoreBssId, context.BssId);
        Services.Assets().UpdateAssetAliasBssIds(context.CatStoreBssId, context.BssId);
        Services.Customers().UpdateCustomerBssId(context.CatStoreBssId, context.BssId);
      }
      catch (Exception ex)
      {
        return Exception(ex, FailureMessage, context.CatStoreBssId, context.BssId);
      }

      //AddEventMessage(inputs, ActivityHelper.GetCustomerCreatedMessage(context));

      return Success(SuccessMessage, context.CatStoreBssId, context.BssId);
    }
  }
}
