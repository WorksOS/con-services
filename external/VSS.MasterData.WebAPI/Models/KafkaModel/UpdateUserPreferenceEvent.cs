using System;

namespace VSS.MasterData.WebAPI.KafkaModel
{
	public class UpdateUserPreferenceEvent
	{
		public Guid? UserUID { get; set; }

		public Guid? PreferenceKeyUID { get; set; }

		public string PreferenceKeyName { get; set; }

		public string PreferenceJson { get; set; }

		public string SchemaVersion { get; set; }

		public DateTime ActionUTC { get; set; }
	}
}