using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Raptor.Service.Common.Filters.Validation
{
  /// <summary>
  /// Validates the passed asset ID.
  /// </summary>
  /// 
  [AttributeUsage(AttributeTargets.Property)]
  public class ValidAssetIDAttribute : ValidationAttribute
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
      long assetID = (long)value;

      if (assetID > 0)
        return ValidationResult.Success;
      else
        return new ValidationResult(string.Format("Invalid asset ID: {0}", assetID));
    }
  }
}