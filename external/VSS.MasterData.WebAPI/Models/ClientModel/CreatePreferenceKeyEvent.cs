using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CreatePreferenceKeyEvent
	{
		[Required]
		public string PreferenceKeyName { get; set; }

		[Required]
		public Guid? PreferenceKeyUID { get; set; }

		[Required]
		public DateTime? ActionUTC { get; set; }
	}
}