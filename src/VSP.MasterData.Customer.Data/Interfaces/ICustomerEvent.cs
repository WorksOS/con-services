using System;

namespace VSP.MasterData.Customer.Data.Interfaces
{
  public interface ICustomerEvent
  {
    Guid CustomerUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}