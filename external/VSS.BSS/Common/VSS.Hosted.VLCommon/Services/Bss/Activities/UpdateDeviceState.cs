using System;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class UpdateDeviceState : Activity
  {
    public const string SUCCESS_MESSAGE = @"State successfuly update for Device: {0}.";
    public const string EXCEPTION_MESSAGE = @"Failed to update state for the Device: {0}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceServiceContext>();

      try
      {
        Services.Devices().UpdateDeviceState(context.ExistingDeviceAsset.DeviceId, DeviceStateEnum.Subscribed);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.IBKey);
      }

      AddEventMessage(inputs, new DeviceSubscribedEvent
                                                      {
                                                        Source = (int) EventSourceEnum.NhBss,
                                                        CreatedUtc = DateTime.UtcNow,
                                                        AssetId = ActivityHelper.GetAssetId(context.IBKey),
                                                        OwnerId = (context.ExistingDeviceAsset.OwnerBSSID == null) ? 0 : Services.Customers().GetCustomerByBssId(context.ExistingDeviceAsset.OwnerBSSID).ID,
                                                        StartDate = context.ActionUTC,  // TODO: Revisit for correct dates
                                                        EndDate = DotNetExtensions.NullKeyDate.FromKeyDate()
                                                      });

      return Success(SUCCESS_MESSAGE, context.IBKey);
    }
  }
}
