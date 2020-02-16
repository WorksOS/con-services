using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface ICustomerAssetEvent
  {
    Guid CustomerUID { get; set; }
    Guid AssetUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}
