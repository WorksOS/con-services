using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.Common.Filters.Validation
{
  /// <summary>
  /// Validates the passed asset ID.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class ValidAssetIDAttribute : ValidationAttribute
  {
    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    ///    shortAssetIds are now generated from the Uid, and can be negative. 
    /// </summary>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      var assetId = (long)value;

      return assetId != 0
        ? ValidationResult.Success
        : new ValidationResult($"Invalid asset ID: {assetId}");
    }
  }
}
