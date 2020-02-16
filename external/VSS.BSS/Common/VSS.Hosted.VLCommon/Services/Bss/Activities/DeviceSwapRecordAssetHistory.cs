using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceSwapRecordAssetHistory : Activity
  {
    public const string SUCCESS_MESSAGE = @"DeviceAssetHistory created for IBDevice {0} ID: {1} removed from Asset ID: {2} Starting: {3} and Ending: {4}";
    public const string RETURN_NULL_MESSAGE = @"Failed to create DeviceAssetHistory. Service returned null for unknown reason.";
    public const string EXCEPTION_MESSAGE = @"Failed to create DeviceAssetHistory.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceAssetContext>();

      AssetDeviceHistory assetDeviceHistory;

      try
      {
        assetDeviceHistory = Services.AssetDeviceHistory().CreateAssetDeviceHistory(
              context.OldDeviceAsset.AssetId,
              context.OldDeviceAsset.DeviceId,
              context.OldDeviceAsset.OwnerBSSID,
              context.OldDeviceAsset.InsertUTC.Value);

        if (assetDeviceHistory == null)
          return Error(RETURN_NULL_MESSAGE);

        AddSummary(SUCCESS_MESSAGE,
              context.OldDeviceAsset.Type.Value,
              assetDeviceHistory.fk_DeviceID,
              assetDeviceHistory.fk_AssetID,
              assetDeviceHistory.StartUTC,
              assetDeviceHistory.EndUTC);

        assetDeviceHistory = Services.AssetDeviceHistory().CreateAssetDeviceHistory(
              context.NewDeviceAsset.AssetId,
              context.NewDeviceAsset.DeviceId,
              context.NewDeviceAsset.OwnerBSSID,
              context.NewDeviceAsset.InsertUTC.Value);

        if (assetDeviceHistory == null)
          return Error(RETURN_NULL_MESSAGE);

        AddSummary(SUCCESS_MESSAGE,
              context.NewDeviceAsset.Type.Value,
              assetDeviceHistory.fk_DeviceID,
              assetDeviceHistory.fk_AssetID,
              assetDeviceHistory.StartUTC,
              assetDeviceHistory.EndUTC);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE);
      }
      return Success();
    }
  }
}
