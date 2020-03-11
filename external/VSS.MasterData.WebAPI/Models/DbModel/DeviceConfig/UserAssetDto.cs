using System;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace DbModel.DeviceConfig
{
    public class UserAssetDto
    {
        private Guid _assetUID;
        public Guid AssetUID
        {
            get { return this._assetUID; }
            set { this._assetUID = value; }
        }
        public string AssetUIDString
        {
            get { return this._assetUID.ToStringWithoutHyphens(); }
            set { this._assetUID = Guid.Parse(value); }
        }
        public int StatusInd { get; set; }
        public string UserUIDString { get; set; }
        public string CustomerUIDString { get; set; }
        public string TypeName { get; set; }
    }
}
