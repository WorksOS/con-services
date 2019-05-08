using System;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Preference
{
	public class UpdatePreferenceKeyEvent : IPreferenceEvent
	{
		public Guid PreferenceKeyUID { get; set; }
		public string PreferenceKeyName { get; set;  } //Required Field
		public DateTime ActionUTC { get; set; }
	}
}