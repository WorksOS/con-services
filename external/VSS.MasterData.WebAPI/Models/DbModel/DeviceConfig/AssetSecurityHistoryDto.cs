using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.Data.MySql;

namespace DbModel.DeviceConfig
{
    public class AssetSecurityHistoryDto : IDbTable
    {
        private string _assetUIDString;

		private static Dictionary<string, string> _columns = new Dictionary<string, string>
		{
			{ "AssetUID", "fk_AssetUID" },
			{ "UserUID", "fk_UserUID" },
			{ "SecurityStatusId", "SecurityStatusId" },
			{ "SecurityModeId", "SecurityModeId" },
			{ "StatusUpdateUTC", "StatusUpdateUTC" },
			{ "RowUpdatedUTC", "RowUpdatedUTC" }
		};

		private static string _tableName = "AssetSecurityHist";

		private static string _idColumn = "AssetSecurityHistID";

		private static string _ignoreUpdateColumns = "AssetSecurityHistID,UserUID";

		public string AssetUIDString
        {
            get { return this._assetUIDString; }
            set { this._assetUIDString = value; }
        }
        public Guid AssetUID
        {
            get { return Guid.Parse(this._assetUIDString); }
            set { this._assetUIDString = value.ToString("N"); }
        }
        private string _userUIDString;
        public string UserUIDString
        {
            get { return this._userUIDString; }
            set { this._userUIDString = value; }
        }
        public Guid UserUID
        {
            get { return Guid.Parse(this._userUIDString); }
            set { this._userUIDString = value.ToString("N"); }
        }
        public ulong AssetSecurityHistID { get; set; }
        public int? SecurityStatusId { get; set; }
        public int? SecurityModeId { get; set; }
        public DateTime StatusUpdateUTC { get; set; }
        public DateTime RowUpdatedUTC { get; set; }

		public string GetTableName()
		{
			return _tableName;
		}

		public string GetIdColumn()
		{
			return _idColumn;
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			return _ignoreUpdateColumns;
		}

		public Dictionary<string, string> GetColumnNames()
		{
			return _columns;
		}
	}
}
