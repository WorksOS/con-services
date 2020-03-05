namespace VSS.MasterData.WebAPI.Utilities.Enums
{
	public class Enums
	{
		public enum MySqlType
		{
			Numeric,
			Boolean,
			String,
			DateTime,
			Guid,
			Others
		}

		public enum DataLinkEnum
		{
			None = 0,
			CDL = 1,
			J1939 = 2,
			ATA = 3,
			SAEJ1939 = 4
		}

		public enum Tenant
		{
			CAT = 1,
			TABGroup = 2,
			J1939Default = 3
		}

		public enum Operation
		{
			Create = 0,
			Update = 1,
			Delete = 2
		}
		public enum DeviceStateEnum
		{
			None = 0,
			Installed = 1,
			Provisioned = 2,
			Subscribed = 3,
			DeregisteredTechnician = 4,
			DeregisteredStore = 5
		}
	}
}
