using System.Linq;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetDeviceContextUpdateValidator : Validator<AssetDeviceContext>
  {
    public override void Validate(AssetDeviceContext context)
    {
      // Failure - Device does not exist to be updated.
      if (!context.Device.Exists)
        AddError(BssFailureCode.IbKeyDoesNotExist, BssConstants.IBKEY_DOES_NOT_EXISTS, context.IBDevice.IbKey);

      // Failure - The IBKey/GPSDeviceId combination in the message is
      // not the same as the IBKey/GPSDeviceId in VL
      var gpsDeviceID = (context.IBDevice.Type == DeviceTypeEnum.CrossCheck && context.IBDevice.GpsDeviceId.StartsWith("0")) ? context.IBDevice.GpsDeviceId.Remove(0, 1) : context.IBDevice.GpsDeviceId;
      if (!context.Device.GpsDeviceId.IsStringEqual(gpsDeviceID))
        AddError(BssFailureCode.GpsDeviceIdInvalid, BssConstants.InstallBase.GPS_DEVICEIDS_DO_NOT_MATCH, context.Device.GpsDeviceId, context.IBDevice.GpsDeviceId);

      // Failure - If the message has ImpliedAction of DeviceReplacement
      // <PrevEquipmentSN>[Different from EquipmentSN]</PrevEquipmentSN>
      // <DeviceState>Active</DeviceState>
      // <PrevDeviceState>NotActive</PrevDeviceState>
      // and it is not a DeviceReplacement scenario
      if (context.ImpliedAction == BssImpliedAction.DeviceReplacement && !context.IsDeviceReplacement())
        AddError(BssFailureCode.DeviceReplaceNotValid, BssConstants.InstallBase.IMPLIED_ACTION_IS_DEVICE_REPLACEMENT);

      // Failure - If the message has ImpliedAction of DeviceTransfer
      // <PrevEquipmentSN>[Different from EquipmentSN]</PrevEquipmentSN>
      // <DeviceState>NotActive</DeviceState>
      // <PrevDeviceState>NotActive</PrevDeviceState>
      // and it is not a DeviceTransfer scenario
      if (context.ImpliedAction == BssImpliedAction.DeviceTransfer && !context.IsDeviceTransfer())
        AddError(BssFailureCode.DeviceTransferNotValid, BssConstants.InstallBase.IMPLIED_ACTION_IS_DEVICE_TRANSFER);
      
      // Failure - Attempting a Device Transfer when the IB Device is active.
      if(context.IsDeviceTransfer() && context.Device.Exists && context.Device.DeviceState == DeviceStateEnum.Subscribed)
        AddError(BssFailureCode.ActiveServiceExistsForDevice, BssConstants.InstallBase.ACTIVE_SERVICE_EXISTS_FOR_DEVICE_ACTION_NOT_VALID, "Transfer", context.Device.IbKey);

      // Failure - Attempting to Device Replacement when the IB Device is active
      if (context.IsDeviceReplacement() && context.Device.Exists && context.Device.DeviceState == DeviceStateEnum.Subscribed)
        AddError(BssFailureCode.ActiveServiceExistsForDevice, BssConstants.InstallBase.ACTIVE_SERVICE_EXISTS_FOR_DEVICE_ACTION_NOT_VALID, "Replacement", context.IBDevice.IbKey);
      
      // Failure - Attempting an Ownership Transfer of an an 
      // active device to a different Registered Dealer.
      // It is valid to transfer the active device if 
      // the current device owner is inactive 
      // OR there is no currently Registered Dealer.
      if (context.IsOwnershipTransfer() && 
         context.Device.Owner.IsActive &&
         context.Device.Owner.RegisteredDealerId.IsDefined() &&
         context.Device.Owner.RegisteredDealerId != context.Owner.RegisteredDealerId &&
         context.Device.DeviceState == DeviceStateEnum.Subscribed)
        AddError(BssFailureCode.ActiveDeviceRegisteredDlrXfer, BssConstants.InstallBase.DEVICE_WITH_ACTIVE_SERVICE_TRANSFER_TO_DIFFERENT_REGISTERED_DEALER);

      // Failure - Attempting a Device Replacement when the IB Device doesn't support
      // the active services on the device being replaced.
      if (context.IsDeviceReplacement() && !Services.ServiceViews().IsDeviceTransferValid(context.Asset.DeviceId, context.IBDevice.Type.Value))
        AddError(BssFailureCode.DeviceReplaceNotValid, BssConstants.DeviceReplacement.NEW_DEVICE_DOES_NOT_SUPPORT_OLD_DEVICE_SERVICES, context.IBDevice.IbKey, context.Device.IbKey);

      if (context.IsDeviceReplacement() && Services.ServiceViews().DeviceHasAnActiveService(context.Device.Id))
        AddError(BssFailureCode.ActiveServiceExistsForDevice, BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, context.Device.IbKey);

      if (context.Device.Exists && context.Device.Asset != null && context.Device.Asset.StoreID != (int)StoreEnum.NoStore && context.Device.Asset.StoreID != (int)StoreEnum.Trimble)
        AddError(BssFailureCode.DeviceRelatedToDifferentStore, BssConstants.InstallBase.BSS_DEVICE_UNAUTHORIZED, context.IBDevice.IbKey);

      if (context.Asset.Exists && context.Asset.StoreID != (int)StoreEnum.NoStore && context.Asset.StoreID != (int)StoreEnum.Trimble)
        AddError(BssFailureCode.AssetRelatedToDifferentStore, BssConstants.InstallBase.BSS_ASSET_UNAUTHORIZED, context.Asset.SerialNumber, context.Asset.MakeCode);
    }
  }
}
