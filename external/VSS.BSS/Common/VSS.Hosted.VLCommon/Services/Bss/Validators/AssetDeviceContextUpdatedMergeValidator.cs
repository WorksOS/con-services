

namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetDeviceContextUpdatedMergeValidator : Validator<AssetDeviceContext>
  {
    public override void Validate(AssetDeviceContext context)
    {

      if(!context.Device.Exists)
        AddError(BssFailureCode.IbKeyDoesNotExist, BssConstants.IBKEY_DOES_NOT_EXISTS, context.IBDevice.IbKey);

      var gpsDeviceID = (context.IBDevice.Type == DeviceTypeEnum.CrossCheck && context.IBDevice.GpsDeviceId.StartsWith("0")) ? context.IBDevice.GpsDeviceId.Remove(0, 1) : context.IBDevice.GpsDeviceId;
      if(!context.Device.GpsDeviceId.IsStringEqual(gpsDeviceID))
        AddError(BssFailureCode.GpsDeviceIdInvalid, BssConstants.InstallBase.GPS_DEVICEIDS_DO_NOT_MATCH, context.Device.GpsDeviceId, context.IBDevice.GpsDeviceId);

      if(context.Device.OwnerBssId.IsStringEqual(context.Owner.BssId))
        AddError(BssFailureCode.DeviceOwnerUnchanged, BssConstants.InstallBase.DEVICE_OWNER_NOT_UPDATED_DURING_MERGE);

      if(context.IsDeviceTransfer() && context.IsOwnershipTransfer())
        AddError(BssFailureCode.DeviceXferAndOwnershipXfer, BssConstants.InstallBase.DEVICE_TRANSFER_AND_OWNSERSHIP_TRANSFER);

      if(context.Device.Owner.Type != context.Owner.Type)
        AddError(BssFailureCode.MergeXferDiffCustomerType, BssConstants.InstallBase.OWNERSHIP_XFER_DUE_TO_MERGE_TO_DIFFERENT_CUSTOMER_TYPE);

      if(context.Device.Owner.RegisteredDealerId.IsDefined() && 
         context.Device.Owner.RegisteredDealerNetwork != context.Owner.RegisteredDealerNetwork )
        AddError(BssFailureCode.MergeXferDiffDealerNetwork, BssConstants.InstallBase.OWNERSHIP_XFER_DUE_TO_MERGE_TO_DIFFERENT_DEALER_NETWORK);
    }
  }
}
