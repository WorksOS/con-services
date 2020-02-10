using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class UpdateUserPreferencePayload:IValidatableObject
	{
		public Guid? PreferenceKeyUID { get; set; }

		[Required]
		public string PreferenceKeyName { get; set; }

		[Required]
		public string PreferenceJson { get; set; }

		public Guid? TargetUserUID { get; set; }

		public string SchemaVersion { get; set; }

		[Required]
		public DateTime? ActionUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (PreferenceKeyUID.HasValue && PreferenceKeyUID == Guid.Empty)
				yield return new ValidationResult("Required field values must be valid.");
		}
	}
}