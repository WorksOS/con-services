using System;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class InstallBaseDataContractValidator : DataContractValidator<InstallBase>
  {
    public override void Validate(InstallBase message)
    {
      Require.IsNotNull(message, "InstallBase");

      base.Validate(message);

      if (!message.IBKey.isNumeric())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_VALID);

      if (!message.OwnerBSSID.isNumeric())
        AddError(BssFailureCode.OwnerBssIdInalid, BssConstants.InstallBase.OWNER_BSSID_NOT_VALID);

      // ModelYear, if defined, must be positive number
      if (message.ModelYear.IsDefined() && 
         (!message.ModelYear.isNumeric() || 
         (message.ModelYear.isNumeric() && int.Parse(message.ModelYear) < 0)))
        AddError(BssFailureCode.ModelyearInvalid, BssConstants.InstallBase.MODEL_YEAR_NOT_VALID);
    }
  }
}
