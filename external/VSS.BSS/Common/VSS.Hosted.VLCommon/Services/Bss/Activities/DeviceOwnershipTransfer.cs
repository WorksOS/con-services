using System;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceTransferOwnership : Activity
  {
    public const string CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE = @"Ownership Transfer cancelled. The device does not exist. Please check the workflow logic.";
    public const string CANCELLED_CURRENT_AND_OWNER_SAME_MESSAGE = @"Ownership Transfer cancelled. The device's current owner and proposed new owner are the same. Please check the workflow logic.";
    public const string RETURNED_FALSE_MESSAGE = @"Failed to Transfer Ownership. Service returned false for unknown reason.";
    public const string EXCEPTION_MESSAGE = @"Failed to Transfer Ownership.";
    public const string SUCCESS_MESSAGE = @"Transfered Ownership of {0} ID: {1} from {2} {3} ID: {5} BSSID: {4} to {6} {7} ID: {9} BSSID: {8}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      if (!context.Device.Exists)
        return Cancelled(CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE);

      if (context.Device.OwnerId == context.Owner.Id)
        return Cancelled(CANCELLED_CURRENT_AND_OWNER_SAME_MESSAGE);

      try
      {
        var success = Services.Devices().TransferOwnership(context.Device.Id, context.Owner.BssId);

        if (!success)
          return Error(RETURNED_FALSE_MESSAGE);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE);
      }

      CustomerTypeEnum oldOwnerType = context.Device.Owner.Type;
      string oldOwnerName = context.Device.Owner.Name;
      string oldOwnerBssId = context.Device.OwnerBssId;
      long oldOwnerId = context.Device.OwnerId;

      // We've transfered the device ownership
      // Update the context to reflect the new ownership
      context.Device.OwnerId = context.Owner.Id;
      context.Device.Owner.MapOwnerDto(context.Owner);

      AddEventMessage(inputs, ActivityHelper.GetDeviceOwnershipTransferredMessage(context, oldOwnerId));

      return Success(SUCCESS_MESSAGE,
          context.Device.Type,
          context.Device.Id,
          oldOwnerType,
          oldOwnerName,
          oldOwnerId,
          oldOwnerBssId,
          context.Owner.Type,
          context.Owner.Name,
          context.Owner.Id,
          context.Owner.BssId);
    }
  }
}
