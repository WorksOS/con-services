using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DeleteAssetPayload //: IValidatableObject
	{
		[Required]
		public Guid? AssetUID { get; set; }
		
		public DateTime? ActionUTC { get; set; }

		public DateTime? ReceivedUTC { get; set; }
		//public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		//{
		//	if (AssetUID == null || AssetUID == Guid.Empty)
		//		yield return new ValidationResult("Required field values must be valid.");
		//}
	}
}