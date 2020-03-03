using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.Customer.KafkaModel
{
	public class AssociateCustomerAssetEvent : IValidatableObject
	{
		[Required]
		public Guid CustomerUID { get; set; }
		[Required]
		public Guid AssetUID { get; set; }
		[Required]
		public string RelationType { get; set; }
		[Required]
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (CustomerUID == Guid.Empty)
			{
				yield return new ValidationResult("CustomerUID field is required.");
			}
			if (AssetUID == Guid.Empty)
			{
				yield return new ValidationResult("AssetUID field is required.");
			}
			if (ActionUTC == DateTime.MinValue)
			{
				yield return new ValidationResult("ActionUTC field is required.");
			}
			if (string.IsNullOrEmpty(RelationType))
			{
				yield return new ValidationResult("RelationType field is required.");
			}
		}
	}
}