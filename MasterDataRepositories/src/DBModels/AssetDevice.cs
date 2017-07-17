using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class AssetDevice
    {
        public string DeviceUID { get; set; }
        public string AssetUID { get; set; }
        public DateTime? LastActionedUtc { get; set; }


        public override bool Equals(object obj)
        {
            var otherAssetDevice = obj as AssetDevice;
            if (otherAssetDevice == null) return false;
            return otherAssetDevice.DeviceUID == DeviceUID
                   && otherAssetDevice.AssetUID == AssetUID
                   && otherAssetDevice.LastActionedUtc == LastActionedUtc;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}