using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  ///     Attribute to test if supplied date greater than referenced.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class DateGreaterThanAttribute : ValidationAttribute
  {
      /// <summary>
      /// Initializes a new instance of the <see cref="DateGreaterThanAttribute"/> class.
      /// </summary>
      /// <param name="dateToCompareToFieldName">Name of the date to compare to field.</param>
      public DateGreaterThanAttribute(string dateToCompareToFieldName)
      {
          DateToCompareToFieldName = dateToCompareToFieldName;
      }

      /// <summary>
      /// Gets the name of the date to compare to field.
      /// </summary>
      /// <value>
      /// The name of the date to compare to field.
      /// </value>
      public string DateToCompareToFieldName { get; }

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
          DateTime earlierDate = (DateTime) value;

          DateTime laterDate =
                  (DateTime)
                          validationContext.ObjectType.GetProperty(DateToCompareToFieldName)
                                  .GetValue(validationContext.ObjectInstance, null);

          if (laterDate < earlierDate)
          {
              return ValidationResult.Success;
          }
          return new ValidationResult("End date must be later than start date.");
      }
  }
 
}