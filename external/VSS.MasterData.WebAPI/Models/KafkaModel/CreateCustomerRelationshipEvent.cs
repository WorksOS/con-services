using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.Customer.KafkaModel
{
	public class CreateCustomerRelationshipEvent : IValidatableObject
	{
		public Guid? ParentCustomerUID { get; set; }
		[Required]
		public Guid ChildCustomerUID { get; set; }
		public Guid? AccountCustomerUID { get; set; }
		[Required]
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (ChildCustomerUID == Guid.Empty)
			{
				yield return new ValidationResult("ChildCustomerUID field is required.");
			}
			if (ActionUTC == DateTime.MinValue)
			{
				yield return new ValidationResult("ActionUTC field is required.");
			}
		}
	}
}