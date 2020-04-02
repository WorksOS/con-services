using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class Device
    {
        public string DeviceUID { get; set; }
        
        // todoMaverick 
        // This IS really an asset id and Raptor treats this as such
        // However in WorksManager there is no asset type entity
        // so I'll stuff it in the Device for now.....
        public int ShortRaptorAssetID { get; set; }
     
        public DateTime? LastActionedUTC { get; set; }

        public override bool Equals(object obj)
        {
            var otherDevice = obj as Device;
            if (otherDevice == null) return false;
            return otherDevice.DeviceUID == DeviceUID
                   && otherDevice.ShortRaptorAssetID == ShortRaptorAssetID
                   && otherDevice.LastActionedUTC == LastActionedUTC;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
