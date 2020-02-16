namespace VSS.Hosted.VLCommon.Bss
{
  public class ServicePlanUpdatedValidator : Validator<DeviceServiceContext>
  {
    public override void Validate(DeviceServiceContext context)
    {
      Require.IsNotNull(context, "DeviceServiceContext");
      Require.IsNotNull(context.ExistingDeviceAsset, "DeviceServiceContext.ExistingDeviceAssetDto");
      Require.IsNotNull(context.ExistingService, "DeviceServiceContext.ExistingServiceDto");

      if (context.ServiceTerminationDate.HasValue)
        AddError(BssFailureCode.ServiceCancelDateDefined, BssConstants.ServicePlan.SERVICE_TERMINATION_DATE, string.Empty, "Updated");

      if (context.ExistingDeviceAsset.OwnerBSSID.IsNotDefined())
        AddError(BssFailureCode.OwnerBssIdDoesNotExist, BssConstants.DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER, context.IBKey);

      if (!context.ExistingDeviceAsset.AssetExists)
        AddError(BssFailureCode.AssetDoesNotExist, BssConstants.ServicePlan.DEVICE_NOT_ASSOCIATED_WITH_ASSET, context.IBKey);

      //invalid - service plan line id does not exists for cancelled and updated action
      if (!context.ExistingService.ServiceExists)
        AddError(BssFailureCode.ServiceDoesNotExist, BssConstants.ServicePlan.SERVICE_DOES_NOT_EXISTS, context.PlanLineID);

      //invalid - service plan updates cannot happen on a cancelled service
       if(!context.IsDeviceDeregistered() && context.ExistingDeviceAsset.DeviceState != DeviceStateEnum.Subscribed)
         AddError(BssFailureCode.ActionInvalid, BssConstants.ServicePlan.ACTION_NOTALLOWED_ON_CANCELLEDSERVICE, context.PlanLineID);

      //invalid - Servce is not associated to the device
      if (context.ExistingDeviceAsset.DeviceId != context.ExistingService.DeviceID)
        AddError(BssFailureCode.ServiceNotAssociatedWithDevice,
          BssConstants.ServicePlan.SERVICE_NOT_ASSOCIATED_WITH_DEVICE, context.PlanLineID,
          context.ExistingService.GPSDeviceID, context.ExistingService.IBKey);
    }
  }
}
