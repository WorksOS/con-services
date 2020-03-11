using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.WebAPI.Utilities.Attributes;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class UpdateProjectSubscriptionEvent : IValidatableObject
	{
		[Required]
		public Guid? SubscriptionUID { get; set; }

		[DbFieldName("fk_CustomerUID")]
		public Guid? CustomerUID { get; set; }

		public string SubscriptionType { get; set; }

		[DbFieldName("StartDate")]
		public DateTime? StartDate { get; set; }

		[DbFieldName("EndDate")]
		public DateTime? EndDate { get; set; }

		[Required]
		public DateTime? ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }

		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
		{
			if (SubscriptionUID == Guid.Empty || CustomerUID == Guid.Empty)
				yield return new ValidationResult("Required field values must be valid.");
		}
	}
}