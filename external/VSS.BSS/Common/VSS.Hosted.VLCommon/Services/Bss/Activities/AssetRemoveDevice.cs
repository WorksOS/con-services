using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  /// <summary>
  /// This activity is executed to remove an IBAsset's currently installed Device.
  /// Before the IBDevice can be installed on the IBAsset, the IBAsset must have its Device removed.
  /// Note: The IBDevice may also be installed on an Asset, this does not modify that relationship. See DeviceRemoveFromAsset
  /// </summary>
  public class AssetRemoveDevice : Activity
  {
    public const string EXCEPTION_MESSAGE = @"Failed to remove {0} ID: {1} from Asset ID: {2}.";
    public const string RETURNED_FALSE_MESSAGE = @"UpdateAsset returned false for an unknown reason. {0} ID: {1} was not removed from Asset ID: {2}";
    public const string CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE = @"Asset ID: {0} does not have an installed Device. Please check workflow logic.";
    public const string CANCELLED_ASSET_DOES_NOT_EXIST_MESSAGE = @"Asset does not exist. Please check workflow logic.";
    public const string SUCCESS_MESSAGE = @"{0} ID: {1} removed from Asset ID: {2}";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      if (!context.Asset.Exists)
        return Cancelled(CANCELLED_ASSET_DOES_NOT_EXIST_MESSAGE);

      if (!context.Asset.DeviceExists)
        return Cancelled(CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE, context.Asset.AssetId);

      var modifiedProperties = new List<Param> { new Param { Name = "fk_DeviceID", Value = 0 } };

      // Added for US 22511
      modifiedProperties.Add(new Param { Name = "IsEngineStartStopSupported", Value = false });
      try
      {
        bool success = Services.Assets().UpdateAsset(context.Asset.AssetId, modifiedProperties);

        if (!success)
          return Error(RETURNED_FALSE_MESSAGE, context.Asset.Device.Type, context.Asset.DeviceId, context.Asset.AssetId);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.Asset.Device.Type, context.Asset.DeviceId, context.Asset.AssetId);
      }

      long deviceId = context.Asset.DeviceId;
      DeviceTypeEnum? deviceType = context.Asset.Device.Type;

      // Update the context to reflect the Device
      // is no longer installed on an asset.
      context.Asset.DeviceId = 0;
      context.Asset.Device.MapDeviceDto(new DeviceDto());

      return Success(SUCCESS_MESSAGE, deviceType.Value, deviceId, context.Asset.AssetId);
    }
  }
}
