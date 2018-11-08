using System;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  ///     Test if supplied list of points contains more than two points and less that 50 points in it.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class MoreThanTwoPointsAttribute : ValidationAttribute
  {
    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>
    /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
    /// </returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      WGSPoint3D[] pointList = (WGSPoint3D[]) value;
      if (pointList.Length < 3)
      {
        return new ValidationResult("A polygon boundary must contain at least 3 points.");
      }
      if (pointList.Length > 50)
      {
        return new ValidationResult("A polygon boundary can contain a maximum of 50 points.");
      }
      return ValidationResult.Success;
    }
  }
}
