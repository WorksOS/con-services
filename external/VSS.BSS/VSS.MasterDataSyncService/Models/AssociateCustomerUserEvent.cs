using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class AssociateCustomerUserEvent : ICustomerUserEvent
  {
    public Guid CustomerUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
