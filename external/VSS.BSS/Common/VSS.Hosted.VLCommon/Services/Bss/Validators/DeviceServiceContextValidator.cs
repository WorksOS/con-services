using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceServiceContextValidator : Validator<DeviceServiceContext>
  {
    public override void Validate(DeviceServiceContext context)
    {
      Require.IsNotNull(context, "DeviceServiceContext");
      Require.IsNotNull(context.ExistingDeviceAsset, "DeviceServiceContext.ExistingDeviceAssetDto");
      Require.IsNotNull(context.ExistingService, "DeviceServiceContext.ExistingServiceDto");

      if (!context.ExistingDeviceAsset.DeviceExists)
        AddError(BssFailureCode.IbKeyDoesNotExist, BssConstants.IBKEY_DOES_NOT_EXISTS, context.IBKey, string.Empty);

      if (context.ExistingService.ServiceExists && context.ServiceType != context.ExistingService.ServiceType)
        AddError(BssFailureCode.ServiceTypesDoesNotMatch, BssConstants.ServicePlan.SERVICE_TYPES_ARE_NOT_EQUAL, 
          context.ServiceType, context.PlanLineID, context.ExistingService.ServiceType);

      if (context.ExistingDeviceAsset.AssetStore != StoreEnum.Trimble && context.ExistingDeviceAsset.AssetStore != StoreEnum.NoStore)
        AddError(BssFailureCode.AssetRelatedToDifferentStore, BssConstants.InstallBase.BSS_ASSET_UNAUTHORIZED, context.ExistingDeviceAsset.AssetSerialNumber, context.ExistingDeviceAsset.AssetMakeCode);
    }
  }
}
