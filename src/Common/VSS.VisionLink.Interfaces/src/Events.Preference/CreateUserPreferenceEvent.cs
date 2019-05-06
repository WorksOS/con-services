using System;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Preference
{
	/// <summary>
	/// Represents a UserPreference object 
	/// </summary>
	public class CreateUserPreferenceEvent : IPreferenceEvent
	{
		public Guid UserUID { get; set; }

		public Guid? PreferenceKeyUID { get; set; }

		public string PreferenceKeyName { get; set; }

		public string PreferenceJson { get; set; }

		public string SchemaVersion { get; set; }

		public DateTime ActionUTC { get; set; }

	}
}
