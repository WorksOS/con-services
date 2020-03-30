using System;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
	/// <summary>
	/// update user preference
	/// </summary>
	public class UpdateUserPreferencePayload
	{
		public Guid? PreferenceKeyUID { get; set; }

		public string PreferenceKeyName { get; set; }

		public string PreferenceJson { get; set; }

		public string SchemaVersion { get; set; }

		public DateTime ActionUTC { get; set; }
	}
}
