using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceAssetSwap : Activity
  {
    public const string SUCCESS_MESSAGE = @"Device swap completed successfully.";
    public const string RETURNED_FALSE_MESSAGE = @"Swapping devices came back false for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to swap Asset {0}, Device: {1} and Asset {2}, Device: {3}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceAssetContext>();
      // Added param IsEngineStartStopSupported for US 22511
      try
      {
        bool success = Services.Assets().UpdateAsset(
          context.OldDeviceAsset.AssetId,
          new List<Param> { new Param { Name = "fk_DeviceID", Value = context.NewDeviceAsset.DeviceId }, new Param { Name = "IsEngineStartStopSupported", Value = false} });

        if (success == false)
          return Error(RETURNED_FALSE_MESSAGE);

        AddSummary("Device Id: {0} successfully installed on Asset Id: {1}.", 
          context.NewDeviceAsset.DeviceId,
          context.OldDeviceAsset.AssetId);

        success = Services.Assets().UpdateAsset(
          context.NewDeviceAsset.AssetId,
          new List<Param> { new Param { Name = "fk_DeviceID", Value = context.OldDeviceAsset.DeviceId }, new Param { Name = "IsEngineStartStopSupported", Value = false} });

        if (success == false)
          return Error(RETURNED_FALSE_MESSAGE);

        AddSummary("Device Id: {0} successfully installed on Asset Id: {1}.",
          context.OldDeviceAsset.DeviceId,
          context.NewDeviceAsset.AssetId);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE,
          context.NewDeviceAsset.AssetId,
          context.OldDeviceAsset.DeviceId,
          context.OldDeviceAsset.AssetId,
          context.NewDeviceAsset.DeviceId);
      }

      return Success(SUCCESS_MESSAGE);
    }
  }
}
