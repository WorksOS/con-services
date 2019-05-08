using System;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Preference
{
	public class DeleteUserPreferenceEvent : IPreferenceEvent
	{
		public Guid? UserUID { get; set; }

		public Guid? PreferenceKeyUID { get; set; }

		public string PreferenceKeyName { get; set; }

		public DateTime ActionUTC { get; set; }
	}
}