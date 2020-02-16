using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IValidator<T>
  {
    IList<string> Warnings { get; }
    void AddWarning(string warningMessage, params object[] values);

    IList<Tuple<BssFailureCode, string>> Errors { get; }
    void AddError(BssFailureCode bssFailureCode, string errorMessage, params object[] values);

    void Validate(T objectToValidate);

    BssFailureCode FirstFailureCode();
  }
}