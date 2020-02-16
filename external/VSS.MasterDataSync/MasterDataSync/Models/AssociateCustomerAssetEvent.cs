using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class AssociateCustomerAssetEvent : ICustomerAssetEvent
  {
    public Guid CustomerUID { get; set; }

    public Guid AssetUID { get; set; }

    public string RelationType { get; set; }

    public DateTime ActionUTC { get; set; }
  }
}
