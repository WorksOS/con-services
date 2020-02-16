using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class CreateAssetSubscriptionEvent : ISubscriptionEvent
  {
    public Guid SubscriptionUID { get; set; }

    public Guid CustomerUID { get; set; }

    public Guid? AssetUID { get; set; }

    public Guid DeviceUID { get; set; }

    public string SubscriptionType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime ActionUTC { get; set; }
  }
}
