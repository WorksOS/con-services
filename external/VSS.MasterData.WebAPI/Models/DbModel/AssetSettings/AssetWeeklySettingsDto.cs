using CommonModel.Enum;
using Data.MySql.Attributes;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace DbModel.AssetSettings
{
	//[DbTable(TableNameOverride = "AssetWeeklyConfig")]
	public class AssetWeeklySettingsDto : IDbTable
    {
        private string _assetWeeklyConfigUIDString;
        private string _assetUIDString;

		private const string _ignoreColumnsOnUpdate = "AssetUID,StartDate,InsertUTC";
		private const string _tableName = "md_asset_AssetWeeklyConfig";
		private const string _idColumn = "AssetWeeklyConfigUID";
		private static Dictionary<string, string> _propertyColumnMap = new Dictionary<string, string>
		{
			{"AssetUID", "fk_AssetUID" },
			{"AssetWeeklyConfigUID", "AssetWeeklyConfigUID" },
			{"AssetConfigTypeID", "fk_AssetConfigTypeID" },
			{"SundayConfigValue", "SundayConfigValue" },
			{"MondayConfigValue", "MondayConfigValue" },
			{"TuesdayConfigValue", "TuesdayConfigValue" },
			{"WednesdayConfigValue", "WednesdayConfigValue" },
			{"ThursdayConfigValue", "ThursdayConfigValue" },
			{"FridayConfigValue", "FridayConfigValue" },
			{"SaturdayConfigValue", "SaturdayConfigValue" },
			{"StartDate", "StartDate" },
			{"EndDate", "EndDate" },
			{"InsertUTC", "InsertUTC" },
			{"UpdateUTC", "UpdateUTC" },
			{"StatusInd", "StatusInd" }
		};

		//[DbIgnore]
		[DBColumnIgnore]
		public IDictionary<AssetTargetType, Tuple<string, double>> TargetValues { get; set; }
		//[DbColumn(ColumnName = "fk_AssetUID"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
		[DBColumnName(Name = "fk_AssetUID")]
		public Guid AssetUID
		{
            get { return Guid.Parse(this._assetUIDString); }
            set { this._assetUIDString = value.ToString("N"); }
        }
        //[DbIgnore]
        //public string AssetUIDString
        //{
        //    get { return _assetUIDString; }
        //    set { _assetUIDString = value; }
        //}
        //[DbUniqueKey, DbColumn(ColumnName = "AssetWeeklyConfigUID"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
        public Guid AssetWeeklyConfigUID { get; set; }
        //[DbIgnore]
        //public string AssetConfigUIDString
        //{
        //    get { return this._assetWeeklyConfigUIDString; }
        //    set { this._assetWeeklyConfigUIDString = value; }
        //}

        //[DbColumn(ColumnName = "fk_AssetConfigTypeID"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
		[DBColumnName(Name = "fk_AssetConfigTypeID")]
        public int AssetConfigTypeID
		{
            get; set;
        }

        //[DbIgnore]
		[DBColumnIgnore]
        public string TargetType { get; set; }


        //[DbColumn(ColumnName = "SundayConfigValue")]
        public double SundayConfigValue { get; set; }
		//[DbColumn(ColumnName = "MondayConfigValue")]
		public double MondayConfigValue { get; set; }
		//[DbColumn(ColumnName = "TuesdayConfigValue")]
		public double TuesdayConfigValue { get; set; }
		//[DbColumn(ColumnName = "WednesdayConfigValue")]
		public double WednesdayConfigValue { get; set; }
		//[DbColumn(ColumnName = "ThursdayConfigValue")]
		public double ThursdayConfigValue { get; set; }
		//[DbColumn(ColumnName = "FridayConfigValue")]
		public double FridayConfigValue { get; set; }
		//[DbColumn(ColumnName = "SaturdayConfigValue")]
		public double SaturdayConfigValue { get; set; }


		//[DbColumn(ColumnName = "StartDate")]
        public DateTime StartDate { get; set; }
        //[DbColumn(ColumnName = "EndDate")]
        public DateTime EndDate { get; set; }
        //[DbColumn(ColumnName = "InsertUTC"), DbIgnore(StatementTypeToIgnore = DbIgnoreAttribute.IgnoreStatementType.Upsert)]
        public DateTime InsertUTC { get; set; }
        //[DbColumn(ColumnName = "UpdateUTC")]
        public DateTime UpdateUTC { get; set; }
		//[DbIgnore]
		[DBColumnIgnore]
		public Guid? UserUID { get; set; }
        //[DbColumn(ColumnName = "StatusInd")]
        public bool StatusInd { get; set; }


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
		public Dictionary<string, string> GetColumnNames()
		{
			return _propertyColumnMap;
		}
	}
}
