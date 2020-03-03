using Data.MySql.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace DbModel.DeviceConfig
{
    public class PingRequestStatus : IDbTable
    {

        private static string _tableName = "md_device_DevicePingLog";

        private static string _idColumn = "DevicePingLogID";

        private static string _ignoreColumnsOnUpdate = "fk_AssetUID,fk_DeviceUID,DevicePingLogUID,RequestTimeUTC";

        private string deviceUID;
        [DBColumnName(Name = "fk_DeviceUID")]
        public Guid DeviceUID
        {
            get
            {
                return Guid.Parse(this.deviceUID);
            }
            set
            {
                this.deviceUID = value.ToString();
            }
        }
        [DBColumnIgnore]
        public string DeviceUIDString
        {
            get
            {
                return this.deviceUID;
            }
            set
            {
                this.deviceUID = value;
            }
        }

        private string assetUID;
        [DBColumnName(Name = "fk_AssetUID")]
        public Guid AssetUID
        {
            get
            {
                return Guid.Parse(this.assetUID);
            }
            set
            {
                this.assetUID = value.ToString();
            }
        }
        [DBColumnIgnore]
        public string AssetUIDString
        {
            get
            {
                return this.assetUID;
            }
            set
            {
                this.assetUID = value;
            }
        }

        private string devicePingLogUID;
        [DBColumnName(Name = "DevicePingLogUID")]
        public Guid DevicePingLogUID
        {
            get
            {
                return Guid.Parse(this.devicePingLogUID);
            }
            set
            {

                this.devicePingLogUID = value.ToString();
            }
        }

        [DBColumnIgnore]
        public string DevicePingLogUIDString
        {
            get
            {
                return this.devicePingLogUID;
            }
            set
            {

                this.devicePingLogUID = value;
            }
        }

        [DBColumnName(Name = "fk_RequestStatusID")]
        public int RequestStatusID { get; set; }

        [DBColumnIgnore]
        public string RequestState { get; set; }

        public DateTime RequestTimeUTC { get; set; }

        public DateTime RequestExpiryTimeUTC { get; set; }

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
            return _ignoreColumnsOnUpdate;
        }

        //[JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        //public IList<IErrorInfo> Errors { get; set; }
    }
}
