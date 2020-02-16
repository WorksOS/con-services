using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  /// <summary>
  /// This Activity is executed when an IB record issues a Device Transfer
  /// and the Device in the message is currently installed on an Asset.
  /// This Activity saves a history of that association. 
  /// It should be executed prior to the DeviceRemoveFromAsset Activity
  /// </summary>
  public class DeviceRemoveFromOldAssetHistory : Activity
  {
    public const string SUCCESS_MESSAGE = @"AssetDeviceHistory created for IBDevice {0} ID: {1} removed from Asset ID: {2} Starting: {3} and Ending: {4}";
    public const string CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE = @"AssetDeviceHistory creation cancelled. Device does not exist. Please check workflow logic.";
    public const string CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE = @"AssetDeviceHistory creation cancelled. Device is not installed on an Asset. Please check workflow logic.";
    public const string RETURN_NULL_MESSAGE = @"Failed to create AssetDeviceHistory. Service returned null for unknown reason.";
    public const string EXCEPTION_MESSAGE = @"Failed to create AssetDeviceHistory.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      if (!context.Device.Exists)
        return Cancelled(CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE);

      if (!context.Device.AssetExists)
        return Cancelled(CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE);

      AssetDeviceHistory assetDeviceHistory;

      try
      {
        assetDeviceHistory = Services.AssetDeviceHistory().CreateAssetDeviceHistory(
          context.Device.AssetId,
          context.Device.Id,
          context.Device.OwnerBssId,
          context.Device.Asset.InsertUtc.Value);

        if (assetDeviceHistory == null)
          return Error(RETURN_NULL_MESSAGE);

      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE);
      }

      return Success(SUCCESS_MESSAGE,
        context.Device.Type,
        assetDeviceHistory.fk_DeviceID,
        assetDeviceHistory.fk_AssetID,
        assetDeviceHistory.StartUTC,
        assetDeviceHistory.EndUTC);
    }
  }
}
