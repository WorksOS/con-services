using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.WebAPI.Utilities.Attributes;
using VSS.MasterData.WebAPI.Utilities.Enums;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class UpdateAssetSubscriptionEvent : IValidatableObject
	{
		[Required]
		public Guid? SubscriptionUID { get; set; }

		[DbFieldName("fk_CustomerUID")]
		public Guid? CustomerUID { get; set; }

		[DbFieldName("fk_AssetUID")]
		public Guid? AssetUID { get; set; }

		[DbFieldName("fk_DeviceUID")]
		public Guid? DeviceUID { get; set; }

		[Required]
		public string SubscriptionType { get; set; }

		[DbFieldName("fk_SubscriptionSourceID", typeof(SubscriptionSource))]
		public string Source { get; set; }

		[DbFieldName("StartDate")]
		public DateTime? StartDate { get; set; }

		[DbFieldName("EndDate")]
		public DateTime? EndDate { get; set; }

		[Required]
		public DateTime? ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }

		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
		{
			if (SubscriptionUID == Guid.Empty)
				yield return new ValidationResult("Required field value must be valid.");
		}
	}
}