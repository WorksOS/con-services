using System.Linq;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class MakeCodeValidator : Validator<InstallBase>
  {
    public override void Validate(InstallBase message)
    {
      if(Data.Context.OP.MakeReadOnly.Count(x => x.Code == message.MakeCode) == 0)
        AddError(BssFailureCode.MakeCodeInvalid, BssConstants.InstallBase.MAKE_CODE_NOT_VALID, message.MakeCode);
    }
  }
}