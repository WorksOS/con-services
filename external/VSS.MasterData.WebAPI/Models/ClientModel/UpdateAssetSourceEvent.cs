using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class AssetSource
	{
		public Guid? AssetUID { get; set; }
		public string Source { get; set; }
	}
	public class UpdateAssetSourceEvent : IValidatableObject
	{
		public UpdateAssetSourceEvent()
		{
			AssetSources = new List<AssetSource>();
		}

		[Required]
		public List<AssetSource> AssetSources { get; set; }

		[Required]
		public DateTime? ActionUTC { get; set; }

		public DateTime? ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if ( ActionUTC == null)
				yield return new ValidationResult("Required field values must be valid.");
			foreach (AssetSource asset in AssetSources)
			{
				if (asset.AssetUID == Guid.Empty|| asset.AssetUID==null || !Guid.TryParse(asset.AssetUID.ToString(), out Guid g))
					yield return new ValidationResult("Required field values must be valid.");
			}
		}
	}
}