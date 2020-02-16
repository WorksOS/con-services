using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface IAssetEvent
  {
    Guid AssetUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}