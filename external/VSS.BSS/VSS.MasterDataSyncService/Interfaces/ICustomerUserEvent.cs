using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface ICustomerUserEvent
  {
    Guid CustomerUID { get; set; }
    Guid UserUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}
