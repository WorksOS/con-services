using System;
using System.Collections.Generic;
using VSS.Nighthawk.MasterDataSync.Interfaces;


namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class AssetECMInfoEvent : IAssetECMInfoEvent
    {
        public Guid AssetUID { get; set; }
        public List<AssetECMInfo> AssetECMInfo { get; set; }
        public string Action { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
