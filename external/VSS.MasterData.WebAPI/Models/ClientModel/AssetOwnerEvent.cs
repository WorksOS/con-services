
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class AssetOwnerEvent :IValidatableObject
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		public Guid? AssetUID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		
		public AssetOwner AssetOwnerRecord { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public Operation Action { get; set; }

		/// <summary>
		/// 
		/// </summary>
	 
		public DateTime? ActionUTC { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (AssetUID== null || AssetUID == Guid.Empty)
				yield return new ValidationResult("AssetUID field values must be valid.");
		} 
	}
}