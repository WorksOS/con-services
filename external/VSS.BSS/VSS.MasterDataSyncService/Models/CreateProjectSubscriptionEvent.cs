using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class CreateProjectSubscriptionEvent : ISubscriptionEvent
  {
    public Guid SubscriptionUID { get; set; }

    public Guid CustomerUID { get; set; }

    public string SubscriptionType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime ActionUTC { get; set; }
  }
}