using Data.MySql.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using VSS.MasterData.WebAPI.Data.MySql;

namespace DbModel.WorkDefinition
{
	public class WorkDefinitionDto : IDbTable
	{
		public static Dictionary<string, string> columns = new Dictionary<string, string>
		{
			{ "AssetUID", "fk_AssetUID" },
			{ "WorkDefinitionTypeID", "fk_WorkDefinitionTypeID" },
			{ "SwitchNumber", "SwitchNumber" },
			{ "SwitchWorkStartState", "SwitchWorkStartState" },
			{ "StartDate", "StartDate" },
			{ "EndDate", "EndDate" },
			{ "InsertUTC", "InsertUTC" },
			{ "UpdateUTC", "UpdateUTC" }
		};

		[DBColumnName(Name = "fk_AssetUID")]
		public Guid AssetUID { get; set; }
		[DBColumnName(Name = "fk_WorkDefinitionTypeID")] 
		public long WorkDefinitionTypeID { get; set; }
		[DBColumnIgnore]
		public string WorkDefinitionType { get; set; }
		public int? SwitchNumber { get; set; }
		public bool? SwitchWorkStartState { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }

		public Dictionary<string, string> GetColumnNames()
		{
			return columns;
		}

		public string GetIdColumn()
		{
			return "fk_AssetUID";
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			return "AssetWorkDefinitionID,WorkDefinitionType";
		}

		public string GetTableName()
		{
			return "md_asset_AssetWorkDefinition";
		}
	}
}
