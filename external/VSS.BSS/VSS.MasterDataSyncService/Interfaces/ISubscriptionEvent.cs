using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface ISubscriptionEvent
  {
    Guid SubscriptionUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}
