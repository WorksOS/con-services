using System;

namespace VSS.Customer.Data.Interfaces
{
  public interface ICustomerEvent
  {
    Guid CustomerUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}