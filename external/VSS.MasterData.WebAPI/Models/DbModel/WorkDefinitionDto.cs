using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class WorkDefinitionDto : IDbTable
	{
		public string fk_AssetUID { get; set; }
		public long fk_WorkDefinitionTypeID { get; set; }
		public int? SwitchNumber { get; set; }
		public bool? SwitchWorkStartState { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }

		public string GetIgnoreColumnsOnUpdate()
		{
			return "AssetWorkDefinitionID,WorkDefinitionType";
		}

		public string GetTableName()
		{
			return "md_asset_WorkDefinitionType";
		}

		public string GetIdColumn()
		{
			return null;
		}
	}
}
