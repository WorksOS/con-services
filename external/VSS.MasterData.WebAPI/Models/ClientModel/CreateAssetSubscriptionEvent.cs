using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CreateAssetSubscriptionEvent : IValidatableObject
	{
		[Required]
		public Guid? SubscriptionUID { get; set; }

		[Required]
		public Guid? CustomerUID { get; set; }

		[Required]
		public Guid? AssetUID { get; set; }

		public Guid? DeviceUID { get; set; }

		[Required]
		public string SubscriptionType { get; set; }

		public string Source { get; set; }

		[Required]
		public DateTime? StartDate { get; set; }

		[Required]
		public DateTime? EndDate { get; set; }

		[Required]
		public DateTime? ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }

		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
		{
			if (SubscriptionUID == Guid.Empty || CustomerUID == Guid.Empty || AssetUID == Guid.Empty)
				yield return new ValidationResult("Required field values must be valid.");
		}
	}
}