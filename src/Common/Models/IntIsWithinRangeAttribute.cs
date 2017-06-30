using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  ///     Tests it supplied value is within the specified range
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class IntIsWithinRangeAttribute : ValidationAttribute
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalIsWithinRangeAttribute"/> class.
    /// </summary>
    /// <param name="lowBoundary">The low boundary.</param>
    /// <param name="highBoundary">The high boundary.</param>
    public IntIsWithinRangeAttribute(int lowBoundary, int highBoundary)
    {
      X = lowBoundary;
      Y = highBoundary;
    }

    /// <summary>
    /// Gets the x.
    /// </summary>
    /// <value>
    /// The x.
    /// </value>
    public int X { get; }
    /// <summary>
    /// Gets the y.
    /// </summary>
    /// <value>
    /// The y.
    /// </value>
    public int Y { get; }

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
      int input = Convert.ToInt32(value);

      if (X <= input && input <= Y)
        return ValidationResult.Success;
      return new ValidationResult(
        $"Supplied value of {(validationContext != null ? validationContext.DisplayName : string.Empty)} should be between {X} and {Y}");
    }
  }
}