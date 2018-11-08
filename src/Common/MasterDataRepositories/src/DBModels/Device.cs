using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class Device
    {
        public string DeviceUID { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string DeviceState { get; set; }
        public DateTime? DeregisteredUTC { get; set; }
        public string ModuleType { get; set; }
        public string MainboardSoftwareVersion { get; set; }
        public string RadioFirmwarePartNumber { get; set; }
        public string GatewayFirmwarePartNumber { get; set; }

        public string DataLinkType { get; set; }

        // apparently this owner is different to the asset ownerCustomerUID
        public string OwningCustomerUID { get; set; }

        public DateTime? LastActionedUtc { get; set; }

        public override bool Equals(object obj)
        {
            var otherAsset = obj as Device;
            if (otherAsset == null) return false;
            return otherAsset.DeviceUID == DeviceUID
                   && otherAsset.DeviceSerialNumber == DeviceSerialNumber
                   && otherAsset.DeviceType == DeviceType
                   && otherAsset.DeviceState == DeviceState
                   && otherAsset.DeregisteredUTC == DeregisteredUTC
                   && otherAsset.ModuleType == ModuleType
                   && otherAsset.MainboardSoftwareVersion == MainboardSoftwareVersion
                   && otherAsset.RadioFirmwarePartNumber == RadioFirmwarePartNumber
                   && otherAsset.GatewayFirmwarePartNumber == GatewayFirmwarePartNumber
                   && otherAsset.DataLinkType == DataLinkType
                   && otherAsset.OwningCustomerUID == OwningCustomerUID
                   && otherAsset.LastActionedUtc == LastActionedUtc;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}