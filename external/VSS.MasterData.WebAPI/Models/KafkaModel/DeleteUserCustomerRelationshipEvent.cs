using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.Customer.KafkaModel
{
	public class DeleteUserCustomerRelationshipEvent : IValidatableObject
	{
		[Required]
		public Guid CustomerUID { get; set; }
		[Required]
		public Guid UserUID { get; set; }
		[Required]
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (CustomerUID == Guid.Empty)
			{
				yield return new ValidationResult("CustomerUID field is required.");
			}
			if (UserUID == Guid.Empty)
			{
				yield return new ValidationResult("UserUID field is required.");
			}
			if (ActionUTC == DateTime.MinValue)
			{
				yield return new ValidationResult("ActionUTC field is required.");
			}
		}
	}
}