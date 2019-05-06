using System;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Preference
{
	public class DeletePreferenceKeyEvent : IPreferenceEvent
	{
		public Guid PreferenceKeyUID { get; set; }
		public DateTime ActionUTC { get; set; }
	}
}
