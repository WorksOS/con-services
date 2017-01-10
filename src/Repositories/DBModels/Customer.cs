using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Customer.Data.Models
{
  public class Customer
  {
    public string Name { get; set; }
    public CustomerType CustomerType { get; set; }
    public string CustomerUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
