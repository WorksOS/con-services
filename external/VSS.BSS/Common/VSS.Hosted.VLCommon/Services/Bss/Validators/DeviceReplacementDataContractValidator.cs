using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceReplacementDataContractValidator : DataContractValidator<DeviceReplacement>
  {
    public override void Validate(DeviceReplacement message)
    {
      base.Validate(message);

      if (message.Action.isStringWithNoSpaces() && !BssMessageAction.IsValidForMessage(message.Action, message))
        AddError(BssFailureCode.ActionInvalid, BssConstants.ACTION_INVALID_FOR_MESSAGE, message.Action, "DeviceReplacement");

      if (!message.OldIBKey.isNumeric())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_VALID);

      if (message.OldIBKey.isNumeric() && Convert.ToInt64(message.OldIBKey).IsNotDefined())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_DEFINED);

      if (!message.NewIBKey.isNumeric())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_VALID);

      if (message.NewIBKey.isNumeric() && Convert.ToInt64(message.NewIBKey).IsNotDefined())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_DEFINED);
    }
  }
}
