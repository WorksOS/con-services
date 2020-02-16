using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  /// <summary>
  /// This activity is executed to remove an IBDevice from it's existing installed Asset.
  /// Before the IBDevice can be installed on the IBAsset, it must be removed from its current Asset.
  /// Note: The IBAsset may also have an installed Device, this does not modify that relationship. See AssetRemoveDevice
  /// </summary>
  public class DeviceRemoveFromOldAsset : Activity
  {
    public const string EXCEPTION_MESSAGE = @"Failed to remove {0} ID: {1} from Asset ID: {2}.";
    public const string RETURNED_FALSE_MESSAGE = @"UpdateAsset returned false for an unknown reason. {0} ID: {1} was not removed from Asset ID: {2}";
    public const string CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE = @"{0} ID: {1} is not installed on an Asset. Please check workflow logic.";
    public const string CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE = @"Device does not exist. Please check workflow logic.";
    public const string SUCCESS_MESSAGE = @"{0} ID: {1} removed from Asset ID: {2}";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      if (!context.Device.Exists)
        return Cancelled(CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE);

      if (!context.Device.AssetExists)
        return Cancelled(CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE, context.Device.Type, context.Device.Id);

      var modifiedProperties = new List<Param> { new Param { Name = "fk_DeviceID", Value = 0 } };

      // Added for US 22511
      modifiedProperties.Add(new Param { Name = "IsEngineStartStopSupported", Value = false });
      long assetId = context.Device.AssetId;

      try
      {
        bool success = Services.Assets().UpdateAsset(context.Device.AssetId, modifiedProperties);

        if (!success)
          return Error(RETURNED_FALSE_MESSAGE, context.Device.Type, context.Device.Id, assetId);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.Device.Type, context.Device.Id, assetId);
      }

      // Update the context to reflect the Device
      // is no longer installed on an asset.
      context.Device.AssetId = 0;
      context.Device.Asset.MapAssetDto(new AssetDto());

      return Success(SUCCESS_MESSAGE, context.Device.Type, context.Device.Id, assetId);
    }
  }
}
