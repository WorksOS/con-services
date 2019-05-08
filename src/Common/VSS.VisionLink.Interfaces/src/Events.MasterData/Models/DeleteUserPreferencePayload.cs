using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
	/// <summary>
	/// delete user preference
	/// </summary>
	public class DeleteUserPreferencePayload
	{
		public Guid? PreferenceKeyUID { get; set; }

		public string PreferenceKeyName { get; set; }
	}
}