namespace VSS.Hosted.VLCommon.Bss
{
  public class ServicePlanCancelledValidator : Validator<DeviceServiceContext>
  {
    public override void Validate(DeviceServiceContext context)
    {
      //invalid - owner visibility date defined for cancelled action
      if (context.OwnerVisibilityDate.HasValue)
        AddError(BssFailureCode.OwnerVisibilityDateDefined, BssConstants.ServicePlan.OWNER_VISIBILITY_DATE, string.Empty, "Cancelled");

      //invalid - service termination date not defined for cancelled action
      if (!context.ServiceTerminationDate.HasValue)
        AddError(BssFailureCode.ServiceCancelDateNotDefined, BssConstants.ServicePlan.SERVICE_TERMINATION_DATE, "not", "Cancelled");

      //invalid - termination date is prior to the activation date
      if (context.ServiceTerminationDate.KeyDate() < context.ExistingService.ActivationKeyDate)
        AddError(BssFailureCode.ServiceCancelDateBeforeActDate,
          BssConstants.ServicePlan.SERVICE_TERMINATION_DATE_IS_PRIOR_TO_ACTIVATION_DATE,
          context.ServiceTerminationDate.KeyDate(), context.ExistingService.ActivationKeyDate);

      //invalid - trying to cancel a service which is already cancelled.
      if (context.ExistingService.ServiceExists 
           && context.ExistingService.CancellationKeyDate <= context.ServiceTerminationDate.KeyDate())
        AddError(BssFailureCode.ServceTerminationInvalid,
          BssConstants.ServicePlan.SERVICE_TERMINATION_NOT_VALID,
          context.ServiceTerminationDate.KeyDate(), context.ExistingService.CancellationKeyDate);

      //invalid - service plan line id does not exists for cancelled and updated action
      if (!context.ExistingService.ServiceExists)
        AddError(BssFailureCode.ServiceDoesNotExist, BssConstants.ServicePlan.SERVICE_DOES_NOT_EXISTS, context.PlanLineID);

      //invalid - Servce is not associated to the device
      if (context.ExistingDeviceAsset.DeviceId != context.ExistingService.DeviceID)
        AddError(BssFailureCode.ServiceNotAssociatedWithDevice,
          BssConstants.ServicePlan.SERVICE_NOT_ASSOCIATED_WITH_DEVICE, context.PlanLineID,
          context.ExistingService.GPSDeviceID, context.ExistingService.IBKey);
    }
  }
}
