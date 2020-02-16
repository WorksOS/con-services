
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetDeviceContextValidator : Validator<AssetDeviceContext>
  {
    public override void Validate(AssetDeviceContext context)
    {
      if(!context.IBDevice.Type.HasValue)
        AddError(BssFailureCode.PartNumberDoesNotExist, BssConstants.InstallBase.PART_NUMBER_DOES_NOT_EXIST, context.IBDevice.PartNumber);

      if(!context.Owner.Exists)
        AddError(BssFailureCode.OwnerBssIdDoesNotExist, BssConstants.InstallBase.OWNER_BSSID_DOES_NOT_EXIST, context.IBDevice.OwnerBssId);

      if(!context.IsValidDeviceOwner())
        AddError(BssFailureCode.DeviceOwnerTypeInvalid, BssConstants.InstallBase.DEVICE_OWNER_TYPE_INVALID, context.Owner.Type);

      if (context.IBDevice.Type != DeviceTypeEnum.MANUALDEVICE && context.IBDevice.GpsDeviceId.IsNotDefined() && (context.IBDevice.Type == null ? true : !AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)context.IBDevice.Type), AppFeatureEnum.SupportBlankGPSDeviceID)))
        AddError(BssFailureCode.GpsDeviceIdNotDefined, BssConstants.InstallBase.GPS_DEVICEID_NOT_DEFINED);
    
      if(context.IBDevice.Type == DeviceTypeEnum.MANUALDEVICE && context.IBDevice.GpsDeviceId.IsDefined())
        AddError(BssFailureCode.GpsDeviceIdDefined, BssConstants.InstallBase.GPS_DEVICEID_DEFINED_MANUAL_DEVICE);
    }
  }
}