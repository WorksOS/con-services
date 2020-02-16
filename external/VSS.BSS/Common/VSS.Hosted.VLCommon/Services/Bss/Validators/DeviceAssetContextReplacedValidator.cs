
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceAssetContextReplacedValidator : Validator<DeviceAssetContext>
  {
    public override void Validate(DeviceAssetContext context)
    {
      // Failure - New Device can not be the same as the Old Device
      if (context.NewDeviceAsset.IbKey.IsStringEqual(context.OldDeviceAsset.IbKey))
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.DeviceReplacement.OLD_IBKEY_AND_NEW_IBKEY_ARE_EQUAL, context.OldIBKey, context.NewIBKey);

      // Failure - New Device is not installed on an Asset or the Old Device has not been removed by a prior IB message.
      if(!context.NewDeviceAsset.AssetExists || context.OldDeviceAsset.AssetExists)
        AddError(BssFailureCode.DeviceReplaceNotValid, BssConstants.DeviceReplacement.NEW_DEVICE_NOT_INSTALLED_OR_OLD_DEVICE_NOT_REMOVED);

      // Failure - New Device must be inactive.
      if (context.NewDeviceAsset.DeviceState == DeviceStateEnum.Subscribed)
        AddError(BssFailureCode.NewDeviceHasServices, BssConstants.DeviceReplacement.NEW_DEVICE_HAS_ACTIVE_SERVICES, context.NewIBKey);

      // Failure - Old Device must be active.
      if (context.OldDeviceAsset.DeviceState != DeviceStateEnum.Subscribed)
        AddError(BssFailureCode.DeviceReplaceNotValid, BssConstants.DeviceReplacement.OLD_DEVICE_DOES_NOT_HAVE_ACTIVE_SERVICE, context.OldIBKey);

      // Failure - New Device does not support the 
      // currently active services of Old Device.
      if (!Services.ServiceViews().IsDeviceTransferValid(context.OldDeviceAsset.DeviceId, context.NewDeviceAsset.Type ?? DeviceTypeEnum.MANUALDEVICE))
        AddError(BssFailureCode.DeviceReplaceNotValid, BssConstants.DeviceReplacement.NEW_DEVICE_DOES_NOT_SUPPORT_OLD_DEVICE_SERVICES, context.NewIBKey, context.OldIBKey);
    }
  }
}
