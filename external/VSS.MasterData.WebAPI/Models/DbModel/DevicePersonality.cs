using System;

namespace VSS.MasterData.WebAPI.DbModels
{
	public class DevicePersonality
	{
		public DevicePersonality(Guid deviceUID, int personalityType, string personalityDesc, string personalityVal)
		{
			fk_DeviceUID = deviceUID;
			DevicePersonalityUID = Guid.NewGuid();
			fk_PersonalityTypeID = personalityType;
			PersonalityDesc = personalityDesc;
			PersonalityValue = personalityVal;
		}
		public DevicePersonality(string DevicePersonalityUIDString, string fk_DeviceUIDString, int fk_PersonalityTypeID, string PersonalityDesc, string PersonalityValue)
		{
			DevicePersonalityUID = Guid.Parse(DevicePersonalityUIDString);
			this.fk_PersonalityTypeID = fk_PersonalityTypeID;
			fk_DeviceUID = Guid.Parse(fk_DeviceUIDString);
			this.PersonalityDesc = PersonalityDesc;
			this.PersonalityValue = PersonalityValue;
		}

		public Guid DevicePersonalityUID { get; set; }
		public Guid fk_DeviceUID { get; set; }
		public int fk_PersonalityTypeID { get; set; }
		public string PersonalityDesc { get; set; }
		public string PersonalityValue { get; set; }
		public DateTime RowUpdatedUTC { get; set; }
	}
}
