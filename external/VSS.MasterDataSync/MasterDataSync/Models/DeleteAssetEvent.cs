using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class DeleteAssetEvent : IAssetEvent
  {
    public Guid AssetUID { get; set; }

    public DateTime ActionUTC { get; set; }

  }
}