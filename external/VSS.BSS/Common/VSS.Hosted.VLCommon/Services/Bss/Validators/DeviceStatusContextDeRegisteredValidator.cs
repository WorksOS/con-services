using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceStatusContextDeRegisteredValidator : Validator<DeviceStatusContext>
  {
    public override void Validate(DeviceStatusContext context)
    {
      //if the device is already in deregistered state, return an error message
      if (context.DeviceAsset.DeviceState == DeviceStateEnum.DeregisteredStore || context.DeviceAsset.DeviceState == DeviceStateEnum.DeregisteredTechnician)
      {
        AddError(BssFailureCode.DeviceAlreadyDeRegistered, BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, context.IBKey, "DeRegistered");
        return;
      }

      //if the status is not either DEREG_STORE, or DEREG_TECH for device deregistration message, return an error message
      if (context.Status.Equals(DeviceRegistrationStatusEnum.REG.ToString(), StringComparison.InvariantCultureIgnoreCase))
      {
        AddError(BssFailureCode.DeviceRegistrationStatusInvald, BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, context.Status, "DeRegistered");
        return;
      }

      //if device has any active service plan, return an error message
      if (Services.ServiceViews().DeviceHasAnActiveService(context.DeviceAsset.DeviceId))
      {
        AddError(BssFailureCode.ActiveServiceExistsForDevice, BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, context.IBKey);
        return;
      }
    }
  }
}
