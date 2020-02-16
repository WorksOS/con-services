using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceStatusContextRegisteredValidator : Validator<DeviceStatusContext>
  {
    public override void Validate(DeviceStatusContext context)
    {
      //invalid - register device action is not valid on a device with is either subscribed or provisioned already.
      if (context.DeviceAsset.DeviceState == DeviceStateEnum.Subscribed || context.DeviceAsset.DeviceState == DeviceStateEnum.Provisioned)
      {
        AddError(BssFailureCode.DeviceRegistrationStateInvalid, BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, context.IBKey, "Registered");
        return;
      }
      //invalid - if the status of the message is not REG for device registered action
      if(!context.Status.Equals(DeviceRegistrationStatusEnum.REG.ToString()))
      {
        AddError(BssFailureCode.DeviceRegistrationStatusInvald, BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, context.Status, "Registered");
        return;
      }
    }
  }
}
