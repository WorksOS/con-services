using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.WebAPI.Utilities.Attributes;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class UpdateCustomerEvent : IValidatableObject
	{
		[DbFieldName("CustomerName")]
		public string CustomerName { get; set; }
		[DbFieldName("BSSID")]
		public string BSSID { get; set; }
		[DbFieldName("DealerNetwork")]
		public string DealerNetwork { get; set; }
		[DbFieldName("NetworkDealerCode")]
		public string NetworkDealerCode { get; set; }
		[DbFieldName("NetworkCustomerCode")]
		public string NetworkCustomerCode { get; set; }
		[DbFieldName("DealerAccountCode")]
		public string DealerAccountCode { get; set; }
		[DbFieldName("PrimaryContactEmail")]
		public string PrimaryContactEmail { get; set; }
		[DbFieldName("FirstName")]
		public string FirstName { get; set; }
		[DbFieldName("LastName")]
		public string LastName { get; set; }
		[Required]
		public Guid CustomerUID { get; set; }
		[DbFieldName("IsActive")]
		public bool? IsActive { get; set; }
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
		}
	}
}