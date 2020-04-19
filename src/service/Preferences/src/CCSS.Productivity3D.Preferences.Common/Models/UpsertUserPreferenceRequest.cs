using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CCSS.Productivity3D.Preferences.Common.Models
{
  public class UpsertUserPreferenceRequest : IValidatableObject
  {
    public Guid? PreferenceKeyUID { get; set; }

    [Required]
    public string PreferenceKeyName { get; set; }

    [Required]
    public string PreferenceJson { get; set; }

    public Guid? TargetUserUID { get; set; }

    public string SchemaVersion { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (PreferenceKeyUID.HasValue && PreferenceKeyUID == Guid.Empty)
        yield return new ValidationResult("Invalid PreferenceKeyUID");
    }
  }
}
