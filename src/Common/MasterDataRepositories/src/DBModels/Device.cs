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
        public int ShortRaptorAssetId { get; set; }
     
        public DateTime? LastActionedUtc { get; set; }

        public override bool Equals(object obj)
        {
            var otherAsset = obj as Device;
            if (otherAsset == null) return false;
            return otherAsset.DeviceUID == DeviceUID
                   && otherAsset.LastActionedUtc == LastActionedUtc;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
