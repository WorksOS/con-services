using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CreateCustomerEvent : IValidatableObject
	{
		[Required]
		public string CustomerName { get; set; }
		[Required]
		public string CustomerType { get; set; }
		public string BSSID { get; set; }
		[Required]
		public string DealerNetwork { get; set; }
		public string NetworkDealerCode { get; set; }
		public string NetworkCustomerCode { get; set; }
		public string DealerAccountCode { get; set; }
		public string PrimaryContactEmail { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		[Required]
		public bool? IsActive { get; set; }
		[Required]
		public Guid CustomerUID { get; set; }
		[Required]
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (CustomerUID == Guid.Empty)
			{
				yield return new ValidationResult("CustomerUID field is required.");
			}
			if (ActionUTC == DateTime.MinValue)
			{
				yield return new ValidationResult("ActionUTC field is required.");
			}
			if (string.IsNullOrEmpty(CustomerType))
			{
				yield return new ValidationResult("CustomerType field is required.");
			}
			if (string.IsNullOrEmpty(DealerNetwork))
			{
				yield return new ValidationResult("DealerNetwork field is required.");
			}
			if (string.IsNullOrEmpty(CustomerName))
			{
				yield return new ValidationResult("CustomerName field is required.");
			}
			if (string.IsNullOrEmpty(BSSID))
			{
				yield return new ValidationResult("BSSID field is required.");
			}
			if (IsActive == null)
			{
				yield return new ValidationResult("IsActive field is required.");
			}
		}
	}
}