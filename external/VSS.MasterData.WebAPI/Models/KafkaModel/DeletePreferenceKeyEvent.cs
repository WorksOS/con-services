using System;

namespace VSS.MasterData.WebAPI.KafkaModel
{
	public class DeletePreferenceKeyEvent
	{
		public Guid PreferenceKeyUID { get; set; }

		public DateTime ActionUTC { get; set; }
	}
}