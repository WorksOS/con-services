using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceTransferOwnershipHistory : Activity
  {
    public const string SUCCESS_MESSAGE = @"AssetDeviceHistory created for AssetID: {0} DeviceID: {1} OwnerBSSID: {2} Starting: {3} and Ending: {4}";
    public const string CANCELLED_ASSET_DOES_NOT_EXIST_MESSAGE = @"AssetDeviceHistory creation cancelled. Asset does not exist. Please check workflow logic.";
    public const string CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE = @"AssetDeviceHistory creation cancelled. Device is not installed on an Asset. Please check workflow logic.";
    public const string RETURN_NULL_MESSAGE = @"Failed to create AssetDeviceHistory. Service returned null for unknown reason.";
    public const string EXCEPTION_MESSAGE = @"Failed to create AssetDeviceHistory.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      if (!context.Asset.Exists)
        return Cancelled(CANCELLED_ASSET_DOES_NOT_EXIST_MESSAGE);

      if (!context.Device.AssetExists)
        return Cancelled(CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE);

      AssetDeviceHistory assetDeviceHistory;
      try
      {
        assetDeviceHistory = Services.AssetDeviceHistory().CreateAssetDeviceHistory(
                context.Asset.AssetId,
                context.Device.Id,
                context.Device.OwnerBssId,
                context.Device.Asset.InsertUtc.Value);

      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE);
      }

      if (assetDeviceHistory == null)
        return Error(RETURN_NULL_MESSAGE);

      return Success(SUCCESS_MESSAGE,
        assetDeviceHistory.fk_AssetID,
        assetDeviceHistory.fk_DeviceID,
        assetDeviceHistory.OwnerBSSID,
        assetDeviceHistory.StartUTC,
        assetDeviceHistory.EndUTC);
    }
  }
}
