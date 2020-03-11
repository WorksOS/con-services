using System;
using System.Collections.Generic;
using System.Text;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel.Device
{
	public class DbDeviceType: IDbTable
	{
		public int DeviceTypeID { get; set; }
		public string TypeName { get; set; }
		public int fk_DeviceTypeFamilyID { get; set; }
		public string DefaultValueJson { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }

		public string GetIgnoreColumnsOnUpdate()
		{
			return "InsertUTC,DeviceTypeID,TypeName";
		}

		public string GetTableName()
		{
			return "DeviceType";
		}
		

		public string GetIdColumn()
		{
			return null;
		}
	}
}
