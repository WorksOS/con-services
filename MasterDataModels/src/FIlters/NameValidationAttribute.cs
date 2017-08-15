using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.Models.FIlters
{
  /// <summary>
  /// Tests whether the supplied string value is not null or empty.
  /// </summary>
  /// 
  [AttributeUsage(AttributeTargets.Property)]
  public class NameValidationAttribute : ValidationAttribute
  {
    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>
    /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
    /// </returns>
    /// 
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      if (value is string name)
      {
        if (String.IsNullOrEmpty(name) == false)
          return ValidationResult.Success;
      }

      return new ValidationResult($"Supplied value of {(validationContext != null ? validationContext.DisplayName : string.Empty)} should not be null or empty.");
    }
  }
}
