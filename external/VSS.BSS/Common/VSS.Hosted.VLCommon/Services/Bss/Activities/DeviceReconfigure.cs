using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceReconfigure : Activity
  {
    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceAssetContext>();

      try
      {
        Services.Devices().ReconfigureDevice(
                context.OldDeviceAsset.DeviceId,
                context.OldDeviceAsset.GpsDeviceId,
                context.OldDeviceAsset.Type.Value,
                context.NewDeviceAsset.DeviceId,
                context.NewDeviceAsset.GpsDeviceId,
                context.NewDeviceAsset.Type.Value,
                context.ActionUTC);
        
        return Success("Devices reconfigured.");
      }
      catch (Exception ex)
      {
        return Exception(ex, "There was an issue when attempting to reconfigure the devices.");
      }
    }
  }
}
