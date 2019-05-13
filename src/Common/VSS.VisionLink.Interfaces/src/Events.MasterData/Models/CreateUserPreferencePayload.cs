using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
	public class CreateUserPreferencePayload
	{
		public Guid? PreferenceKeyUID { get; set; }

        //Will be excluded later
		public Guid? CustomerUID { get; set; }

		public string PreferenceKeyName { get; set; }

		public string PreferenceJson { get; set; }

		public string SchemaVersion { get; set; }

		public DateTime ActionUTC { get; set; }
	}
}