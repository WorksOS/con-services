using System;

namespace VSS.UserCustomer.Data.Interfaces
{
  public interface IUserCustomerEvent
  {
    Guid CustomerUID { get; set; }
    Guid UserUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}