using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetCreate : Activity
  {
    public const string SUCCESS_MESSAGE = @"Created Asset with ID: {0} MakeCode: {1} SerialNumber: {2} Name: {3}. Installed {4} ID: {5} IBKey: {6} on Asset.";
    public const string ASSET_NULL_MESSAGE = @"Creation of Asset came back null for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to create Asset for MakeCode: {0} SerialNumber: {1}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      Asset newAsset;
      try
      {
        newAsset = Services.Assets().CreateAsset(context.IBAsset, context.Device.Id, context.Device.Type);

        if (newAsset == null)
          return Error(ASSET_NULL_MESSAGE);

        // We have successfully created an Asset
        // The context is updated to reflect
        // the new Assets properties
        context.Asset.AssetId = newAsset.AssetID;
        context.Asset.MapAsset(newAsset);

        // The new asset was created with the 
        // Device on the context so update the
        // Asset's installed device with the 
        // Device and the Owner context
        context.Asset.DeviceId = context.Device.Id;
        context.Asset.Device.MapDeviceDto(context.Device);
        context.Asset.DeviceOwnerId = context.Owner.Id;
        context.Asset.DeviceOwner.MapOwnerDto(context.Owner);

        // Lastly, update the Device on the context to reflect the 
        // Device is installed on the new Asset
        context.Device.AssetId = newAsset.AssetID;
        context.Device.Asset.MapAsset(newAsset);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.IBAsset.Name, context.IBAsset.SerialNumber);
      }

      AddEventMessage(inputs, ActivityHelper.GetAssetCreatedMessage(context));

      return Success(SUCCESS_MESSAGE, 
        newAsset.AssetID, 
        newAsset.fk_MakeCode, 
        newAsset.SerialNumberVIN, 
        newAsset.Name, 
        context.Device.Type, 
        context.Device.Id, 
        context.Device.IbKey);
    }
  }
}
