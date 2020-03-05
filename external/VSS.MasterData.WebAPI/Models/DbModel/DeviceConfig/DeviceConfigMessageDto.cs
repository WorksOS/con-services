using Data.MySql.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.Data.MySql;
//using VSS.DB.Attributes;

namespace DbModel.DeviceConfig
{
	//[DbTable(TableNameOverride = "DeviceConfigMessage")]
    public class DeviceConfigMessageDto : IDbTable
    {
		private string _deviceConfigMessageUIDString;
		private string _deviceUIDString;
		private string _userUIDString;

		private static Dictionary<string, string> _columns = new Dictionary<string, string>
		{
			{ "DeviceConfigMessageUID", "DeviceConfigMessageUID" },
			{ "DeviceUID", "fk_DeviceUID" },
			{ "UserUID", "fk_UserUID" },
			{ "DeviceTypeID", "fk_DeviceTypeID" },
			{ "EventUTCString", "EventUTC" },
			{ "MessageContent", "MessageContent" },
			{ "StatusID", "fk_StatusID" },
			{ "LastMessageUTCString", "LastMessageUTC" }
		};

		private static string _tableName = "md_device_DeviceConfigMessage";

		private static string _idColumn = "DeviceConfigMessageId";

		private static string _ignoreColumnsOnUpdate = "DeviceConfigMessageUIDString,DeviceUIDString,UserUIDString,EventUTC,LastMessageUTC,AttributeName,ParameterName,IsPending";

		[DBColumnIgnore]
		public string DeviceConfigMessageUIDString
		{
			get { return _deviceConfigMessageUIDString; }
			set { _deviceConfigMessageUIDString = value; }
		}
		[DBColumnName(Name = "DeviceConfigMessageUID")]
		public Guid DeviceConfigMessageUID
		{
			get { return Guid.Parse(this._deviceConfigMessageUIDString); }
			set { this._deviceConfigMessageUIDString = value.ToString("N"); }
		}
		[DBColumnIgnore]
		public string DeviceUIDString
		{
			get { return _deviceUIDString; }
			set { _deviceUIDString = value; }
		}
		[DBColumnName(Name = "fk_DeviceUID")]
		public Guid DeviceUID
		{
			get { return Guid.Parse(this._deviceUIDString); }
			set { this._deviceUIDString = value.ToString("N"); }
		}
		[DBColumnIgnore]
		public string UserUIDString
		{
			get { return _userUIDString; }
			set { _userUIDString = value; }
		}
		[DBColumnName(Name = "fk_UserUID")]
		public Guid UserUID
		{
			get { return Guid.Parse(this._userUIDString); }
			set { this._userUIDString = value.ToString("N"); }
		}
		[DBColumnName(Name = "fk_DeviceTypeID")]
		public int DeviceTypeID { get; set; }
		[DBColumnName(Name = "EventUTC")]
		public DateTime EventUTC { get { return DateTime.Parse(EventUTCString); } }
		[DBColumnIgnore]
		public string EventUTCString { get;set; }
		[DBColumnName(Name = "MessageContent")]
		public string MessageContent { get; set; }
		[DBColumnName(Name = "fk_StatusID")]
		public int StatusID { get; set; }
		[DBColumnName(Name = "LastMessageUTC")]
		public DateTime LastMessageUTC { get { return DateTime.Parse(LastMessageUTCString); } }
		[DBColumnIgnore]
		public string LastMessageUTCString { get; set; }

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
