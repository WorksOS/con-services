using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface ICustomerEvent
  {
    Guid CustomerUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}