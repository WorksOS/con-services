using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.Common.Filters.Validation
{
  /// <summary>
  /// Validates the passed project UID.
  /// </summary>
  /// 
  [AttributeUsage(AttributeTargets.Property)]
  public class ValidProjectUIDAttribute : ValidationAttribute
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
      var projectUID = (Guid?)value;

      if (!projectUID.HasValue || (projectUID != Guid.Empty))
        return ValidationResult.Success;
      else
        return new ValidationResult(string.Format("Invalid project UID: {0}", projectUID));
    }
  }
}