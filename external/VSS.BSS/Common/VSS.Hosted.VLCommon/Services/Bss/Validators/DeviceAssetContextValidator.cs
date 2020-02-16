namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceAssetContextValidator : Validator<DeviceAssetContext>
  {
    public override void Validate(DeviceAssetContext context)
    {
      if (!context.OldDeviceAsset.DeviceExists)
        AddError(BssFailureCode.IbKeyDoesNotExist, BssConstants.DeviceReplacement.IBKEY_DOES_NOT_EXISTS, context.OldIBKey, "Old");

      if (!context.NewDeviceAsset.DeviceExists)
        AddError(BssFailureCode.IbKeyDoesNotExist, BssConstants.DeviceReplacement.IBKEY_DOES_NOT_EXISTS, context.NewIBKey, "New");
    }
  }
}
