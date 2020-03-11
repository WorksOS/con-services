using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class BulkDissociateCustomerUserEvent : IValidatableObject
	{
		[Required]
		public Guid CustomerUID { get; set; }
		[Required]
		public List<Guid> UserUID { get; set; }
		[Required]
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (CustomerUID == Guid.Empty)
			{
				yield return new ValidationResult("CustomerUID field is required.");
			}
			if (UserUID?.Count < 1)
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