using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.WebAPI.Data.MySql;
//using VSS.DB.Attributes;

namespace DbModel.DeviceConfig
{
	//[DbTable(TableNameOverride = "DevicePingACKMessage")]
    public class DevicePingACKMessageDto : IDbTable
    {
		private string _devicePingACKMessageUIDString;
		private string _deviceUIDString;
        private string _assetUIDString;
        private string _devicePingLogUIDString;
        private string _userUIDString;

        //[DbColumn(ColumnName = "DevicePingACKMessageUID")]
		public string DevicePingACKMessageUID
        {
			get { return this._devicePingACKMessageUIDString; }
			set { this._devicePingACKMessageUIDString = Guid.Parse(value ?? "").ToString("N"); }
		}

        //[DbColumn(ColumnName = "DevicePingLogUID")]
        public string DevicePingLogUID
        {
            get { return this._devicePingLogUIDString; }
            set { this._devicePingLogUIDString = Guid.Parse(value??"").ToString("N"); }
        }

		//[DbColumn(ColumnName = "DeviceUID")]
		public string DeviceUID
		{
            get { return this._deviceUIDString; }
            set { this._deviceUIDString = Guid.Parse(value ?? "").ToString("N"); }
		}

        //[DbColumn(ColumnName = "AssetUID")]
        public string AssetUID
        {
            get { return this._assetUIDString; }
            set { this._assetUIDString = Guid.Parse(value ?? "").ToString("N"); }
        }

		//[DbColumn(ColumnName = "AcknowledgeTimeUTC")]
		public DateTime AcknowledgeTimeUTC { get;set; }

		//[DbColumn(ColumnName = "AcknowledgeStatusID")]
		public int AcknowledgeStatusID { get; set; }

        //[DbColumn(ColumnName = "RowUpdatedUTC")]
		public DateTime RowUpdatedUTC { get; set; }

		public Dictionary<string, string> GetColumnNames()
		{
			throw new NotImplementedException();
		}

		public string GetIdColumn()
		{
			throw new NotImplementedException();
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			throw new NotImplementedException();
		}

		public string GetTableName()
		{
			throw new NotImplementedException();
		}
	}
}
