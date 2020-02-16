using System;
using System.Collections.Generic;
using System.Linq;


namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetUpdate : Activity
  {
    public const string SUCCESS_MESSAGE = @"Updated Asset properties.{0}";
    public const string CANCELLED_MESSAGE = @"Asset update cancelled. There were no modified properties on asset.";
    public const string RETURNED_FALSE_MESSAGE = @"Update of asset properties came back false for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to update Asset {0} properties.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var modifiedProperties = new List<Param>();
      var context = inputs.Get<AssetDeviceContext>();

      Require.IsNotNull(context.IBAsset, "AssetDeviceContext.IBAsset");

      // Updating Asset's installed device
      if (context.Asset.DeviceId != context.Device.Id)
      {
        AddWarning("Installing new {0} IBKey: {1} on Asset", context.Device.Type, context.Device.IbKey);
        modifiedProperties.Add(new Param { Name = "fk_DeviceID", Value = context.Device.Id });

        //Reset EngineOnOff tracking for US 22511 when device is updated
        modifiedProperties.Add(new Param { Name = "IsEngineStartStopSupported", Value = false });
      }

      // The statement below will need to be removed when
      // AssetAlias is fully implemented.
      if(!context.Asset.Name.IsStringEqual(context.IBAsset.Name))
        modifiedProperties.Add(new Param { Name = "Name", Value = context.IBAsset.Name });

      if (!context.Asset.Model.IsStringEqual(context.IBAsset.Model))
        modifiedProperties.Add(new Param {Name = "Model", Value = context.IBAsset.Model});

      if (context.Asset.ManufactureYear != context.IBAsset.ManufactureYear)
        modifiedProperties.Add(new Param { Name = "ManufactureYear", Value = context.IBAsset.ManufactureYear });

      if (context.Asset.AssetVinSN != context.IBAsset.AssetVinSN)
        modifiedProperties.Add(new Param { Name = "EquipmentVIN", Value = context.IBAsset.AssetVinSN });

      if (modifiedProperties.Count() == 0)
        return Cancelled(CANCELLED_MESSAGE);

      try
      {
        if (context.Asset.StoreID != (int)StoreEnum.Trimble)
        {
          context.IBAsset.StoreID = (int)StoreEnum.Trimble;
          context.Asset.StoreID = (int)StoreEnum.Trimble;
          modifiedProperties.Add(new Param { Name = "fk_StoreID", Value = context.IBAsset.StoreID });
        }
        var success = Services.Assets().UpdateAsset(context.Asset.AssetId, modifiedProperties);

        if (!success)
          return Error(RETURNED_FALSE_MESSAGE);

        // We have successfully updated the Asset using the IBAsset properties
        // The context is updated to reflect the updated Assets properties
        context.Asset.MapAssetDto(context.IBAsset);

        // The asset may have been updated with the Device on the context.
        if (context.Asset.DeviceId != context.Device.Id)
        {
          // If so, update the Asset's installed device with the 
          // Device and the Owner context
          context.Asset.DeviceId = context.Device.Id;
          context.Asset.Device.MapDeviceDto(context.Device);
          context.Asset.DeviceOwnerId = context.Owner.Id;
          context.Asset.DeviceOwner.MapOwnerDto(context.Owner);
          
          //// Lastly, update the Device on the context to reflect the 
          //// Device is installed on the new Asset
          //context.Device.AssetId = context.Asset.AssetId;
          //context.Device.Asset.MapAssetDto(context.Asset);
        }
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.Asset.SerialNumber, context.Device.Id);
      }

      AddEventMessage(inputs, ActivityHelper.GetAssetUpdatedMessage(context));

      string summary = modifiedProperties.Select(x => string.Format("\n\tModified {0}: {1}", x.Name, x.Value)).ToFormattedString();
      return Success(SUCCESS_MESSAGE, summary);
    }
  }
}
