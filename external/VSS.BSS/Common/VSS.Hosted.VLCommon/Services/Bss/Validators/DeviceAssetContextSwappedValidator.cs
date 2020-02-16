namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceAssetContextSwappedValidator : Validator<DeviceAssetContext>
  {
    public override void Validate(DeviceAssetContext context)
    {
      if (!context.OldDeviceAsset.OwnerBSSID.IsStringEqual(context.NewDeviceAsset.OwnerBSSID))
        AddError(BssFailureCode.DeviceOwnerChangedForOldAndNew, BssConstants.DeviceReplacement.OWNER_BSSID_DIFFERENT_FOR_OLDIBKEY_AND_NEWIBKEY, context.OldIBKey, context.NewIBKey);

      if (!context.OldDeviceAsset.AssetExists)
        AddError(BssFailureCode.AssetDoesNotExist, BssConstants.ASSET_NOT_ASSOCIATED_WITH_DEVICE, "Old", context.OldIBKey);

      if (!context.NewDeviceAsset.AssetExists)
        AddError(BssFailureCode.AssetDoesNotExist, BssConstants.ASSET_NOT_ASSOCIATED_WITH_DEVICE, "New", context.NewIBKey);
    }
  }
}
