using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Raptor.Service.Common.Filters.Validation
{
  /// <summary>
  /// Validates the passed filter ID.
  /// </summary>
  /// 
  [AttributeUsage(AttributeTargets.Property)]
  public class ValidFilterIDAttribute : ValidationAttribute
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
      long filterID = (long)value;

      if (filterID > 0)
        return ValidationResult.Success;
      else
        return new ValidationResult(string.Format("Invalid filter ID: {0}", filterID));
    }
  }
}