using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon.Bss
{
  public abstract class Validator<T> : IValidator<T>
  {
    
    private IList<string> _warnings = new List<string>();
    private IList<Tuple<BssFailureCode, string>> _errors = new List<Tuple<BssFailureCode, string>>();

    public IList<string> Warnings
    {
      get { return _warnings; }
    }

    public IList<Tuple<BssFailureCode, string>> Errors
    {
      get { return _errors; }
    }

    public void AddError(BssFailureCode bssFailureCode, string errorMessage, params object[] values)
    {
      _errors.Add(new Tuple<BssFailureCode, string>(bssFailureCode, string.Format(errorMessage, values)));
    }

    public void AddWarning(string warningMessage, params object[] values)
    {
      _warnings.Add(string.Format(warningMessage, values));
    }

    public abstract void Validate(T objectToValidate);

    public BssFailureCode FirstFailureCode()
    {
      return Errors.Select(x => x.Item1).FirstOrDefault();
    }
  }
}