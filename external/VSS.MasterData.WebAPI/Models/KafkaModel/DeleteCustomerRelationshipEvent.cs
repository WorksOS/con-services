using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.Customer.KafkaModel
{
	public class DeleteCustomerRelationshipEvent : IValidatableObject
	{
		public Guid? ParentCustomerUID { get; set; } // Nullable  becasue of Root Parent (Corp Customers)
		[Required]
		public Guid ChildCustomerUID { get; set; }
		public Guid? AccountCustomerUID { get; set; }
		[Required]
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (ChildCustomerUID == Guid.Empty
				|| ActionUTC == DateTime.MinValue)
			{
				yield return new ValidationResult("Required field values must be valid.");
			}
		}
	}
}