using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon.Bss
{
  public class Validate<T> : IActivity where T : class
  {
    private readonly IValidator<T>[] _validators;

    public Validate(params IValidator<T>[] validators)
    {
      _validators = validators;
    }

    public ActivityResult Execute(Inputs inputs)
    {
      var objectToValidate = inputs.Get<T>();
      var errors = new List<Tuple<BssFailureCode, string>>();
      var warnings = new List<string>();

      foreach (var validator in _validators)
      {
        validator.Validate(objectToValidate);
        errors.AddRange(validator.Errors);
        warnings.AddRange(validator.Warnings);
      }

      if (errors.Count > 0)
      {
        var firstFailureCode = errors.Select(x => x.Item1).First();
        var formattedErrors = errors.Select(x => x.Item2).ToFormattedString();
        string errorMessage = string.Format(CoreConstants.VALIDATION_FAILED, typeof(T).Name, formattedErrors);
        return new BssErrorResult { FailureCode = firstFailureCode, Summary = errorMessage };
      }

      if (warnings.Count > 0)
      {
        string warningMessage = string.Format(CoreConstants.VALIDATION_PASSED_WITH_WARNINGS, typeof(T).Name, warnings.ToFormattedString());
        return new WarningResult { Summary = warningMessage };
      }

      var result = new ActivityResult { Summary = string.Format(CoreConstants.VALIDATION_PASSED, typeof(T).Name)};
      return result;
    }
  }
}
