using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class DissociateProjectSubscriptionEvent : IValidatableObject
	{
		[Required]
		public Guid SubscriptionUID { get; set; }

		[Required]
		public Guid ProjectUID { get; set; }

		[Required]
		public DateTime? EffectiveDate { get; set; }

		[Required]
		public DateTime? ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (SubscriptionUID == Guid.Empty || ProjectUID == Guid.Empty)
				yield return new ValidationResult("Required field values must be valid.");
		}
	}
}