using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceStatusContextValidator : Validator<DeviceStatusContext>
  {
    public override void Validate(DeviceStatusContext context)
    {
      Require.IsNotNull(context, "DeviceStatusContext");
      Require.IsNotNull(context.DeviceAsset, "DeviceServiceContext.DeviceAsset");

      if (!context.DeviceAsset.DeviceExists)
        AddError(BssFailureCode.IbKeyDoesNotExist, BssConstants.IBKEY_DOES_NOT_EXISTS, context.IBKey, string.Empty);

      if (context.DeviceAsset.Type != null)
      {
        var featureSetId = DeviceTypeList.GetAppFeatureSetId((int)context.DeviceAsset.Type);

        if (context.DeviceAsset.DeviceExists && (!(AppFeatureMap.DoesFeatureSetSupportsFeature(featureSetId, AppFeatureEnum.OTADeregistration))))
          AddError(BssFailureCode.DeviceDeregNotSupported, BssConstants.DeviceRegistration.DEVICE_REGISTRATION_NOT_SUPPORTED, context.DeviceAsset.Type);
      }

      //if(!context.DeviceAsset.AssetExists)
      //  AddError(BssFailureCode.AssetDoesNotExist, BssConstants.ASSET_NOT_ASSOCIATED_WITH_DEVICE, string.Empty, context.IBKey);

      if (context.DeviceAsset.OwnerBSSID.IsNotDefined())
        AddError(BssFailureCode.OwnerBssIdDoesNotExist, BssConstants.DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER, context.IBKey);
    }
  }
}
