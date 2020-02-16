using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceCreate : Activity
  {
    public const string SUCCESS_MESSAGE = @"Created {0} with ID: {1} for IBKey: {2}.";
    public const string DEVICE_NULL_MESSAGE = @"Creation of device came back null for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to create {0} for IBKey: {1}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      Require.IsNotNull(context.IBDevice, "DeviceContext.New");

      Device newDevice;

      try
      {
        newDevice = Services.Devices().CreateDevice(context.IBDevice);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.IBDevice.Type, context.IBDevice.IbKey);
      }

      if (newDevice == null)
        return Error(DEVICE_NULL_MESSAGE);

      // We have successfully created a Device
      // The context is updated to reflect
      // the new device properties
      context.Device.Id = newDevice.ID;
      context.Device.MapDevice(newDevice);

      // Also, the new device was created to reference
      // the Owner on the context so update
      // the Device's Owner with the Owner on the context
      context.Device.OwnerId = context.Owner.Id;
      context.Device.Owner.MapOwnerDto(context.Owner);

      return Success(SUCCESS_MESSAGE, (DeviceTypeEnum) newDevice.fk_DeviceTypeID, newDevice.ID, newDevice.IBKey);
    }
  }
}
