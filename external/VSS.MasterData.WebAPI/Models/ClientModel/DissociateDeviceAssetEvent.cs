using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	/// <summary>
	/// Dissociate device asset event details
	/// </summary>
	public class DissociateDeviceAssetEvent:IValidatableObject
	{
		/// <summary>
		/// Device unique identifier
		/// </summary>
		[Required]
		public Guid DeviceUID { get; set; }
		/// <summary>
		/// Asset unique idnetifier
		/// </summary>
		[Required]
		public Guid AssetUID { get; set; }
		/// <summary>
		/// Dissociate device asset event action UTC time
		/// </summary>
		[Required]
		public DateTime? ActionUTC { get; set; }
		/// <summary>
		/// Dissociate device asset event received UTC time
		/// </summary>
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (ActionUTC == null)
				yield return new ValidationResult("The ActionUTC field is required.");

			if (DeviceUID == null || DeviceUID == Guid.Empty)
				yield return new ValidationResult("The DeviceUID field is required.");

			if (AssetUID == null || AssetUID == Guid.Empty)
				yield return new ValidationResult("The AssetUID field is required.");
		}
	}
}
