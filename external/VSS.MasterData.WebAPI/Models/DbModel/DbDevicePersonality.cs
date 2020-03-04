using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel.Device
{
	public class DbDevicePersonality : IDbTable
	{
		public int DevicePersonalityID { get; set; }
		public Guid DevicePersonalityUID { get; set; }
		public Guid fk_DeviceUID { get; set; }
		public int fk_PersonalityTypeID { get; set; }
		public string PersonalityDesc { get; set; }
		public string PersonalityValue { get; set; }
		public DateTime RowUpdatedUTC { get; set; }
			
		public string GetIgnoreColumnsOnUpdate()
		{
			return "DevicePersonalityID,fk_DeviceUID";
		}

		public string GetTableName()
		{
			return "md_device_DevicePersonality";
		}
		

		public string GetIdColumn()
		{
			return null;
		}
	}
}