using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Raptor.Service.Common.Models
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
    public int X { get; private set; }
    /// <summary>
    /// Gets the y.
    /// </summary>
    /// <value>
    /// The y.
    /// </value>
    public int Y { get; private set; }

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
      return new ValidationResult(String.Format("Supplied value of {0} should be between {1} and {2}", validationContext != null ? validationContext.DisplayName : String.Empty, X, Y));
    }
  }
}