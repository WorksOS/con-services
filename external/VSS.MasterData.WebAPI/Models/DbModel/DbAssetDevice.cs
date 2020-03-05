using System;
using System.Collections.Generic;
using System.Text;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbAssetDevice : IDbTable
	{
		public long AssetDeviceID { get; set; }
		public Guid fk_AssetUID { get; set; }
		public Guid fk_DeviceUID { get; set; }
		public DateTime RowUpdatedUTC { get; set; }
		public DateTime ActionUTC { get; set; }

		public string GetIdColumn()
		{
			throw new NotImplementedException();
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			return "AssetDeviceID";
		}

		public string GetTableName()
		{
			return "md_asset_AssetDevice";
		}
	}
}