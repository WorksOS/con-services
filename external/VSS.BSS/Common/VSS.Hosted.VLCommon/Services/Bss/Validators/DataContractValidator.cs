using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DataContractValidator<TMessage> : Validator<TMessage>
  {
    public override void Validate(TMessage message)
    {
      if (!(message.PropertyValueByName("ControlNumber") as string).isNumeric())
        AddError(BssFailureCode.ControlNumberInvalid, BssConstants.CONTROL_NUMBER_NOT_VALID);

      if (!(message.PropertyValueByName("Action") as string).isStringWithNoSpaces())
        AddError(BssFailureCode.ActionInvalid, BssConstants.ACTION_NOT_VALID);

      if (!(message.PropertyValueByName("ActionUTC") as string).isDateTimeValid())
        AddError(BssFailureCode.ActionUtcInvalid, BssConstants.ACTION_UTC_NOT_VALID);

      if ((message.PropertyValueByName("SequenceNumber") as long?) == 0)
        AddError(BssFailureCode.SequenceNumberNotDefined, BssConstants.SEQUENCE_NUMBER_NOT_DEFINED);

      if ((message.PropertyValueByName("ControlNumber") as string).isNumeric() && (message.PropertyValueByName("ControlNumber") as string) == "0")
        AddError(BssFailureCode.ControlNumberNotDefined, BssConstants.CONTROL_NUMBER_NOT_DEFINED);
    }
  }
}
