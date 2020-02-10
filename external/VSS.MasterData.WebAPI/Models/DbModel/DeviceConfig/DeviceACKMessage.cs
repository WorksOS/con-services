using CommonModel.Helpers;
using System;

namespace DbModel.DeviceConfig
{
    public class DeviceACKMessage
    {
        public string DevicePingACKMessageUID { get; set; }
        public byte[] DevicePingACKMessageID => !string.IsNullOrEmpty(DevicePingACKMessageUID) ? Guid.Parse(DevicePingACKMessageUID).GetByteArrayFromGuid() : Guid.Empty.GetByteArrayFromGuid();
        public string DevicePingLogUID { get; set; }
        public byte[] DevicePingLogID => !string.IsNullOrEmpty(DevicePingLogUID) ? Guid.Parse(DevicePingLogUID).GetByteArrayFromGuid() : Guid.Empty.GetByteArrayFromGuid();
        public string DeviceUID { get; set; }
        public byte[] DeviceID { get { return !string.IsNullOrEmpty(DeviceUID) ? Guid.Parse(DeviceUID).GetByteArrayFromGuid() : Guid.Empty.GetByteArrayFromGuid(); } }
        public string AssetUID { get; set; }
        public byte[] AssetID { get { return !string.IsNullOrEmpty(AssetUID) ? Guid.Parse(AssetUID).GetByteArrayFromGuid() : Guid.Empty.GetByteArrayFromGuid(); } }
        public int AckStatusID { get; set; }
        public DateTime RowUpdatedUTC { get; set; }
    }
}
