using System;
using System.Collections.Generic;
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
    public interface IAssetECMInfoEvent
    {
        Guid AssetUID { get; set; }
        List<AssetECMInfo> AssetECMInfo { get; set; }
        string Action { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}
