using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ResetAssetOnOffTrackingForDeviceReplacement : Activity
  {
    public const string SUCCESS_MESSAGE = @"OnOff tracking rest for old and new assets successfully.";
    public const string RETURNED_FALSE_MESSAGE = @"OnOff tracking rest for old and new assets came back false for unknown reason.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceAssetContext>();
      var modifiedProperties = new List<Param>();

      try
      {
        // Reset AssetOnOff tracking for the old and new assets
        modifiedProperties.Add(new Param { Name = "IsEngineStartStopSupported", Value = false });
        bool success = false;

        if (context.OldDeviceAsset.AssetExists)
        {
          success = Services.Assets().UpdateAsset(context.OldDeviceAsset.AssetId, modifiedProperties); 
        }
        
        if(context.NewDeviceAsset.AssetExists)
        {
          success = Services.Assets().UpdateAsset(context.NewDeviceAsset.AssetId, modifiedProperties); 
        }

        if (success == false)
          return Error(RETURNED_FALSE_MESSAGE);

        AddSummary("OnOff tracking reset completed");

        return Success(SUCCESS_MESSAGE);
      }
      catch (Exception ex)
      {
        return Exception(ex, "There was an issue when resetting the OnOff tracking");
      }
    }
  }
}
