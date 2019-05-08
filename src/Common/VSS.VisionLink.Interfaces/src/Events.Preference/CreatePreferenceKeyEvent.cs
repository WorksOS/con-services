using System;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Preference
{
	public class CreatePreferenceKeyEvent : IPreferenceEvent
	{
		public string PreferenceKeyName { get; set; } //Required Field
		public Guid PreferenceKeyUID { get; set; }
		public DateTime ActionUTC { get; set; }
	}
}