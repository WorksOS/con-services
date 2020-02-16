using System;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeActivateDeviceState : Activity
  {
    public const string SUCCESS_MESSAGE = @"Device: {0} state successfully updated to DeActivate.";
    public const string RETURNED_FALSE_MESSAGE = @"Device deactivation returned false for an unknown reason. Device: {0} not DeActivated.";
    public const string EXCEPTION_MESSAGE = @"Failed to DeActivate Device: {0}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceServiceContext>();
      bool success;

      try
      {
        success = Services.ServiceViews().DeActivateDevice(context.IBKey);

        if (success == false)
          return Error(RETURNED_FALSE_MESSAGE, context.IBKey);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.IBKey);
      }

      AddEventMessage(inputs, new DeviceProvisionedEvent
                                                        {
                                                          Source = (int) EventSourceEnum.NhBss,
                                                          CreatedUtc = DateTime.UtcNow,
                                                          AssetId = ActivityHelper.GetAssetId(context.IBKey),
                                                          OwnerId = (context.ExistingDeviceAsset.OwnerBSSID == null) ? 0 : Services.Customers().GetCustomerByBssId(context.ExistingDeviceAsset.OwnerBSSID).ID,
                                                          StartDate = context.ActionUTC,
                                                          EndDate = context.ServiceTerminationDate.HasValue ? context.ServiceTerminationDate.Value : DateTime.UtcNow
                                                        });

      return Success(SUCCESS_MESSAGE, context.IBKey);
    }
  }
}
