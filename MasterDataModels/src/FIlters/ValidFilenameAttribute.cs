using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace VSS.MasterData.Models.FIlters
{
  /// <summary>
  ///     Tests if supplied string has valid filename
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class ValidFilenameAttribute : ValidationAttribute
  {
    /// <summary>
    /// Gets the maxlength.
    /// </summary>
    /// <value>
    /// The maxlength.
    /// </value>
    public int Maxlength { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidFilenameAttribute"/> class.
    /// </summary>
    /// <param name="maxlength">The maxlength.</param>
    public ValidFilenameAttribute(int maxlength)
    {
      this.Maxlength = maxlength;
    }

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
      string filename = (string)value;

      if (filename.Length > Maxlength || string.IsNullOrEmpty(filename) || filename.IndexOfAny(Path.GetInvalidPathChars()) > 0 || String.IsNullOrEmpty(Path.GetFileName(filename)))
        return new ValidationResult(
          $"Supplied filename is not valid. Exceeds the length limit of {Maxlength}, empty or contains illegal characters.");
      return ValidationResult.Success;
    }
  }
}
