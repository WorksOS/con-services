namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetDeviceContextCreateValidator : Validator<AssetDeviceContext>
  {
    public override void Validate(AssetDeviceContext context)
    {
      if (context.Device.Exists)
        AddError(BssFailureCode.IbKeyExists, BssConstants.InstallBase.IBKEY_EXISTS, context.IBDevice.IbKey);

      if(context.IsDeviceReplacement() && context.IsOwnershipTransfer())
        AddError(BssFailureCode.DeviceReplaceAndOwnershipXfer, BssConstants.InstallBase.DEVICE_REPLACEMENT_AND_OWNERSHIP_TRANSFER);

      if (context.IBDevice.Type != DeviceTypeEnum.MANUALDEVICE && 
          ((context.IBDevice.Type != DeviceTypeEnum.THREEPDATA && Services.Devices().GetDeviceByGpsDeviceId(context.IBDevice.GpsDeviceId, context.IBDevice.Type) != null) 
            || (context.IBDevice.Type == DeviceTypeEnum.THREEPDATA &&   !string.IsNullOrEmpty(context.IBDevice.GpsDeviceId) && Services.Devices().GetDeviceByGpsDeviceId(context.IBDevice.GpsDeviceId, context.IBDevice.Type) != null)))
        AddError(BssFailureCode.GpsDeviceIdExists, BssConstants.InstallBase.GPS_DEVICEID_EXISTS, context.IBDevice.GpsDeviceId);

      if (context.IsDeviceReplacement() && context.IBDevice.Type.HasValue && !Services.ServiceViews().IsDeviceTransferValid(context.Asset.DeviceId, context.IBDevice.Type.Value))
        AddError(BssFailureCode.DeviceReplaceNotValid, BssConstants.DeviceReplacement.NEW_DEVICE_DOES_NOT_SUPPORT_OLD_DEVICE_SERVICES, context.IBDevice.IbKey, context.Asset.Device.IbKey);

      if(context.IsDeviceReplacement() && Services.ServiceViews().DeviceHasAnActiveService(context.Device.Id))
        AddError(BssFailureCode.ActiveServiceExistsForDevice, BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, context.Device.IbKey);

      if (context.Device.Exists && context.Device.Asset != null && context.Device.Asset.StoreID != (int)StoreEnum.NoStore && context.Device.Asset.StoreID != (int)StoreEnum.Trimble)
        AddError(BssFailureCode.DeviceRelatedToDifferentStore, BssConstants.InstallBase.BSS_DEVICE_UNAUTHORIZED, context.IBDevice.IbKey);

      if (context.Asset.Exists && context.Asset.StoreID != (int)StoreEnum.NoStore && context.Asset.StoreID != (int)StoreEnum.Trimble)
        AddError(BssFailureCode.AssetRelatedToDifferentStore, BssConstants.InstallBase.BSS_ASSET_UNAUTHORIZED, context.Asset.SerialNumber, context.Asset.MakeCode);
    }
  }
}
