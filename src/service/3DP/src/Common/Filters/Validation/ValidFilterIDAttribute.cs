using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.Common.Filters.Validation
{
  /// <summary>
  /// Validates the passed filter ID.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class ValidFilterIDAttribute : ValidationAttribute
  {
    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      var filterId = (long)value;

      return filterId > 0
        ? ValidationResult.Success
        : new ValidationResult($"Invalid filter ID: {filterId}");
    }
  }
}
