using System;

namespace VSS.MasterData.WebAPI.KafkaModel
{
	public class DeleteUserPreferenceEvent
	{
		public Guid UserUID { get; set; }

		public Guid? PreferenceKeyUID { get; set; }

		public string PreferenceKeyName { get; set; }

		public DateTime ActionUTC { get; set; }
	}
}