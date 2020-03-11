using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class AssetListParam : IValidatableObject
	{
        public List<string> AssetUIDs { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			bool isInValid = false;
			foreach (string assetUID in AssetUIDs)
				if (!Guid.TryParse(assetUID, out Guid g))
				{
					isInValid = true;
					break;
				}
			if (isInValid)
				yield return new ValidationResult("Required field values must be valid.");

		}
	}
}
