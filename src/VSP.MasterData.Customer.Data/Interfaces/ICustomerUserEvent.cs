using System;

namespace VSS.Customer.Data.Interfaces
{
  public interface ICustomerUserEvent
  {
    Guid CustomerUID { get; set; }
    Guid UserUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}