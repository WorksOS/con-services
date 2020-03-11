using Data.MySql.Attributes;
using CommonModel.Enum;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace DbModel.AssetSettings
{
	//[DbTable(TableNameOverride = "AssetConfig")]
	public class AssetSettingsDto : IDbTable
    {
		//private const string _ignoreColumns = "TargetValues, TargetType, FilterCriteria,CusotmerUid,UserUID";
		private const string _ignoreColumnsOnUpdate = "AssetUID,StartDate,InsertUTC";
		private const string _tableName = "md_asset_AssetConfig";
		private const string _idColumn = "AssetConfigUID";
		private static Dictionary<string, string> _propertyColumnMap = new Dictionary<string, string>
		{
			{"AssetUID", "fk_AssetUID" },
			{"AssetConfigUID", "AssetConfigUID" },
			{"AssetConfigTypeID", "fk_AssetConfigTypeID" },
			{"TargetValue", "ConfigValue" },
			{"StartDate", "StartDate" },
			{"EndDate", "EndDate" },
			{"InsertUTC", "InsertUTC" },
			{"UpdateUTC", "UpdateUTC" },
			{"StatusInd", "StatusInd" }
		};

		//[DbIgnore]
		[DBColumnIgnore]
		public IDictionary<AssetTargetType, Tuple<Guid, double>> TargetValues { get; set; }
        
		//[DbColumn(ColumnName = "fk_AssetUID"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
        [DBColumnName(Name = "fk_AssetUID")]
		public Guid AssetUID { get; set; }

		//[DbIgnore]

        //[DbUniqueKey, DbColumn(ColumnName = "AssetConfigUID")]
        public Guid AssetConfigUID { get; set; }

		//[DbIgnore]

		//[DbColumn(ColumnName = "fk_AssetConfigTypeID"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
		[DBColumnName(Name = "fk_AssetConfigTypeID")]
		public int AssetConfigTypeID { get; set; }
        [DBColumnIgnore]
        public string TargetType { get; set; }
		[DBColumnName(Name = "ConfigValue")]
		public double TargetValue { get; set; }
        //[DbColumn(ColumnName = "StartDate"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
        public DateTime StartDate { get; set; }
        //[DbColumn(ColumnName = "EndDate")]
        public DateTime? EndDate { get; set; }
        //[DbColumn(ColumnName = "InsertUTC"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
        public DateTime InsertUTC { get; set; }
        //[DbColumn(ColumnName = "UpdateUTC")]
        public DateTime UpdateUTC { get; set; }
		//[DbIgnore]
		[DBColumnIgnore]
		public IEnumerable<KeyValuePair<string, Tuple<string, object>>> FilterCriteria { get; set; }
		//[DbIgnore]
		[DBColumnIgnore]
		public Guid? CusotmerUid { get; set; }
		//[DbIgnore]
		[DBColumnIgnore]
		public Guid? UserUID { get; set; }
        //[DbColumn(ColumnName = "StatusInd")]
        public bool StatusInd { get; set; }

		public Dictionary<string, string> GetColumnNames()
		{
			return _propertyColumnMap;
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			return _ignoreColumnsOnUpdate;
		}

		public string GetTableName()
		{
			return _tableName;
		}

		public string GetIdColumn()
		{
			return _idColumn;
		}
	}
}
