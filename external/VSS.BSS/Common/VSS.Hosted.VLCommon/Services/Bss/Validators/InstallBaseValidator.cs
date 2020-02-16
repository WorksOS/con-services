using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class InstallBaseValidator : Validator<InstallBase>
  {
    public override void Validate(InstallBase message)
    {
      Require.IsNotNull(message, "InstallBase");

      if(!BssMessageAction.IsValidForMessage(message.Action, message))
        AddError(BssFailureCode.ActionInvalid, BssConstants.ACTION_INVALID_FOR_MESSAGE, message.Action, "InstallBase");

      if (message.IBKey.IsNotDefined())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_DEFINED);

      if (message.EquipmentSN.IsNotDefined())
        AddError(BssFailureCode.EquipmentSNNotDefined, BssConstants.InstallBase.EQUIPMENTSN_NOT_DEFINED);

      if (message.MakeCode.IsNotDefined())
        AddError(BssFailureCode.MakeCodeNotDefined, BssConstants.InstallBase.MAKE_CODE_NOT_DEFINED);

      if(message.OwnerBSSID.IsNotDefined())
        AddError(BssFailureCode.OwnerBssNotDefined, BssConstants.InstallBase.OWNER_BSSID_NOT_DEFINED);

      if (message.PartNumber.IsNotDefined())
        AddError(BssFailureCode.PartNumberNotDefined, BssConstants.InstallBase.PART_NUMBER_NOT_DEFINED);

      if ((message.EquipmentVIN.IsDefined()) && (message.EquipmentVIN.Length > 50))
        AddError(BssFailureCode.EquipmentVINInvalid, BssConstants.InstallBase.EQUIPMENTVIN_TOO_LONG);
    }
  }
}
