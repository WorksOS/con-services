namespace VSS.MasterData.Repositories.ExtendedModels
{
    public class AssetDeviceIds
    {
        // this is returned from the Device.GetDevicesAsset()
        //   and provides additional values from Asset and Device tables 
        public string DeviceUID { get; set; }

        public string AssetUID { get; set; }

        public long LegacyAssetID { get; set; }
        public string OwningCustomerUID { get; set; }

        public string DeviceType { get; set; }
        public string RadioSerial { get; set; }


        public override bool Equals(object obj)
        {
            var otherAsset = obj as AssetDeviceIds;
            if (otherAsset == null) return false;
            return otherAsset.DeviceUID == DeviceUID
                   && otherAsset.AssetUID == AssetUID
                   && otherAsset.OwningCustomerUID == OwningCustomerUID
                   && otherAsset.DeviceType == DeviceType
                   && otherAsset.RadioSerial == RadioSerial;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}