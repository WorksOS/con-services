using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.Models.Validation
{
  /// <summary>
  /// Validates the passed project UID.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class ValidProjectUIDAttribute : ValidationAttribute
  {
    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.</returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      try
      {
        var projectUid = (Guid?) value;

        if (!projectUid.HasValue || projectUid != Guid.Empty)
        {
          return ValidationResult.Success;
        }
      }
      catch (InvalidCastException)
      {
        // Consume any type casting errors and allow the function to return a failed result.
      }

      return new ValidationResult("Invalid project UID.");
    }
  }
}