using System;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class UserPreference
	{
		public string PreferenceKeyName { get; set; }

		public string PreferenceJson { get; set; }

		public Guid PreferenceKeyUID { get; set; }

		public string SchemaVersion { get; set; }
	}
}