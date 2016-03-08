using Newtonsoft.Json;
using System;

namespace VSP.MasterData.Customer.Data.Models
{
  public class DeleteCustomer
  {
    public Guid CustomerUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
