using System;

namespace VSS.MasterData.Customer.Processor.Interfaces
{
  public interface ICustomerEvent
  {
    Guid CustomerUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}