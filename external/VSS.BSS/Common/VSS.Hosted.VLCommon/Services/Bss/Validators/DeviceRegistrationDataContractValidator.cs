using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceRegistrationDataContractValidator : DataContractValidator<DeviceRegistration>
  {
    public override void Validate(DeviceRegistration message)
    {
      if (message == null)
        throw new ArgumentNullException("message");

      base.Validate(message);

      if (message.Action.isStringWithNoSpaces() && !BssMessageAction.IsValidForMessage(message.Action, message))
        AddError(BssFailureCode.ActionInvalid, BssConstants.ACTION_INVALID_FOR_MESSAGE, message.Action, "DeviceRegistration");

      if (!message.IBKey.isNumeric())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_VALID);

      if (message.Status.IsNotDefined() || !message.Status.isStringWithNoSpaces() || !Enum.GetNames(typeof(DeviceRegistrationStatusEnum)).Contains(message.Status))
        AddError(BssFailureCode.DeviceRegistrationStateInvalid, BssConstants.DeviceRegistration.DEVICE_REGISTRATION_STATUS_NOT_VALID, message.Status);

      if (message.IBKey.isNumeric() && Convert.ToInt64(message.IBKey).IsNotDefined())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_DEFINED);
    }
  }
}
