using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
    public class ServicePlanActivatedValidator : Validator<DeviceServiceContext>
    {
        public override void Validate(DeviceServiceContext context)
        {
            Require.IsNotNull(context, "DeviceServiceContext");
            Require.IsNotNull(context.ExistingDeviceAsset, "DeviceServiceContext.ExistingDeviceAssetDto");
            Require.IsNotNull(context.ExistingService, "DeviceServiceContext.ExistingServiceDto");

            //invalid - service termination date defined for activated/updated action
            if (context.ServiceTerminationDate.HasValue)
                AddError(BssFailureCode.ServiceCancelDateDefined, BssConstants.ServicePlan.SERVICE_TERMINATION_DATE, string.Empty, "Activated");

            //invalid - service type for the part number is not valid
            if (!context.ServiceType.HasValue || context.ServiceType == ServiceTypeEnum.Unknown)
                AddError(BssFailureCode.ServiceTypeDoesNotExists, BssConstants.ServicePlan.SERVICE_TYPE_DOES_NOT_EXISTS, context.PartNumber);

            //invalid - another service exists with the same service plan line id
            if(context.ExistingService.ServiceExists)
                AddError(BssFailureCode.ServiceExists, BssConstants.ServicePlan.SERVICE_EXISTS, context.PlanLineID);

            //invalid - device is not associated to a valid dealer
            if (string.IsNullOrWhiteSpace(context.ExistingDeviceAsset.OwnerBSSID))
                AddError(BssFailureCode.OwnerBssIdDoesNotExist, BssConstants.DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER, context.IBKey);
            
            //invalid - device has active services while performing CAT Daily & CAT Daily in VL subscription
            if ((context.ServiceType == ServiceTypeEnum.VisionLinkDaily || context.ServiceType == ServiceTypeEnum.CATDaily) && Services.ServiceViews().DeviceHasAnActiveService(context.ExistingDeviceAsset.DeviceId))
                AddError(BssFailureCode.ActiveServiceExistsForDevice, BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, context.ExistingDeviceAsset.IbKey);

            //invalid - device type doesn't support the requesting service type
            if (context.ExistingDeviceAsset.DeviceExists && !Services.ServiceViews().DeviceSupportsService(context.ServiceType.Value, context.ExistingDeviceAsset.Type.Value))
                AddError(BssFailureCode.DeviceDoesNotSupportService, BssConstants.ServicePlan.SERVICE_TYPE_NOT_SUPPORTED_FOR_DEVICE_TYPE, context.ServiceType, context.ExistingDeviceAsset.Type);

            //invalid - device has same service plan which is still active with another plan line id
            if (!string.IsNullOrWhiteSpace(context.ExistingService.DifferentServicePlanLineID))
                AddError(BssFailureCode.SameServiceExists, BssConstants.ServicePlan.DEVICE_HAS_SAME_ACTIVE_SERVICE, context.ServiceType, context.IBKey, context.ExistingService.DifferentServicePlanLineID);

            //invalid - device has active CAT Daily subscription
            if (Services.ServiceViews().IsActiveCATDailyServiceExist(context.ExistingDeviceAsset.DeviceId))
                AddError(BssFailureCode.ActiveCATDailyServiceExistsForDevice, BssConstants.ACTIVE_CATDaily_SERVICE_EXISTS_FOR_DEVICE, context.ExistingDeviceAsset.IbKey);

            //invalid - device has existing  active CAT Daily in Visionlink subscription
            if (Services.ServiceViews().IsActiveVisionLinkDailyServiceExist(context.ExistingDeviceAsset.DeviceId))
                AddError(BssFailureCode.ActiveVisionLinkDailyServiceExistsForDevice, BssConstants.ACTIVE_VisionLinkDaily_SERVICE_EXISTS_FOR_DEVICE, context.ExistingDeviceAsset.IbKey);

            //invalid - device is not associated to any valid asset
            if (!context.ExistingDeviceAsset.AssetExists)
                AddError(BssFailureCode.AssetDoesNotExist, BssConstants.ServicePlan.DEVICE_NOT_ASSOCIATED_WITH_ASSET, context.IBKey);
        }
    }
}
