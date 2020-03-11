using Data.MySql.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using VSS.DB.Attributes;
using VSS.MasterData.WebAPI.Data.MySql;

namespace DbModel.DeviceConfig
{
	//[DbTable(TableNameOverride = "DeviceConfig")]
    public class DeviceConfigDto : IDbTable
    {
		private static Dictionary<string, string> _columns = new Dictionary<string, string>
		{
			{ "DeviceUID", "fk_DeviceUID" },
			{ "DeviceTypeParameterID", "fk_DeviceTypeParameterID" },
			{ "DeviceParameterAttributeId", "fk_DeviceParamAttrID" },
			{ "AttributeValue", "AttributeValue" },
			{ "FutureAttributeValue", "FutureAttributeValue" },
			{ "FutureAttrEventUTC", "FutureAttrEventUTC" },
			{ "LastAttrEventUTC", "LastAttrEventUTC" },
			{ "RowInsertedUTC", "InsertUTC" },
			{ "RowUpdatedUTC", "UpdateUTC" }
		};

		private static string _tableName = "md_device_DeviceConfig";

		private static string _idColumn = "DeviceConfigId";

		private static string _ignoreColumnsOnUpdate = "AssetUID,AssetUIDString,DeviceUIDString,DeviceConfigID,DeviceTypeName,AttributeName,ParameterName,IsPending";

		private string _assetUIDString;
		private string _deviceUIDString;

		[DBColumnIgnore]
		public ulong DeviceConfigID { get; set; }

		[DBColumnIgnore]
		public string DeviceUIDString
		{
			get { return this._deviceUIDString.ToLower(); }
			set { this._deviceUIDString = value; }
		}

		[DBColumnName(Name = "fk_DeviceUID")]
		public Guid DeviceUID
		{
			get { return Guid.Parse(this._deviceUIDString); }
			set { this._deviceUIDString = value.ToString("N"); }
		}

		[DBColumnIgnore]
		public string AssetUIDString
        {
            get { return this._assetUIDString.ToLower(); }
            set { this._assetUIDString = value; }
        }

		[DBColumnIgnore]
		public Guid AssetUID
		{
			get { return Guid.Parse(this._assetUIDString); }
			set { this._assetUIDString = value.ToString("N"); }
		}

		[DBColumnName(Name = "fk_DeviceTypeParameterID")]
		public ulong DeviceTypeParameterID { get; set; }
		[DBColumnName(Name = "fk_DeviceParamAttrID")]
		public ulong DeviceParameterAttributeId { get; set; }
		[DBColumnIgnore]
		public string AttributeValue { get; set; }
		[DBColumnIgnore]
		public string CurrentAttributeValue { get; set; }
		[DBColumnName(Name = "FutureAttributeValue")]
		public string FutureAttributeValue { get; set; }
		[DBColumnName(Name = "FutureAttrEventUTC")]
		public string FutureAttrEventUTC { get; set; }
		[DBColumnIgnore]
		public string LastAttrEventUTC { get; set; }
		[DBColumnIgnore] 
		public string RowInsertedUTC { get; set; }
		[DBColumnIgnore] 
		public string RowUpdatedUTC { get; set; }
		[DBColumnIgnore]
		public string UpdateUTC { get; set; }
		[DBColumnIgnore]
		public string AttributeName { get; set; }
		[DBColumnIgnore]
		public string ParameterName { get; set; }
		[DBColumnIgnore]
		public bool IsPending { get; set; }
		[DBColumnIgnore]
		public string DeviceTypeName { get; set; }
		[DBColumnIgnore]
		public DateTime? FutureAttributeEventUTC
		{
			get
			{
				DateTime result;
				if (!string.IsNullOrEmpty(FutureAttrEventUTC) && DateTime.TryParse(FutureAttrEventUTC, out result))
				{
					return result;
				}
				return null;
			}
		}
		[DBColumnIgnore]
		public DateTime? LastAttributeEventUTC
		{
			get
			{
				DateTime result;
				if (!string.IsNullOrEmpty(LastAttrEventUTC) && DateTime.TryParse(LastAttrEventUTC, out result))
				{
					return result;
				}
				return null;
			}
		}
		[DBColumnName(Name = "UpdateUTC")]
		public DateTime? DBUpdateUTC
		{
			get
			{
				DateTime result;
				if (!string.IsNullOrEmpty(RowUpdatedUTC) && DateTime.TryParse(RowUpdatedUTC, out result))
				{
					return result;
				}
				return null;
			}
		}
		[DBColumnName(Name = "InsertUTC")]
		public DateTime? DBInsertUTC
		{
			get
			{
				DateTime result;
				if (!string.IsNullOrEmpty(RowInsertedUTC) && DateTime.TryParse(RowInsertedUTC, out result))
				{
					return result;
				}
				return null;
			}
		}

		public Dictionary<string, string> GetColumnNames()
		{
			return _columns;
		}

		public string GetIdColumn()
		{
			return _idColumn;
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			return _ignoreColumnsOnUpdate;
		}

		public string GetTableName()
		{
			return _tableName;
		}
	}
}
