using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;


namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class AssetOwnerEvent : IAssetOwnerEvent
  {
    public Guid AssetUID { get; set; }
    public AssetOwner AssetOwnerRecord { get; set; }
    public string Action { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
