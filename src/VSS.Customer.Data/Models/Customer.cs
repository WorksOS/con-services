using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Customer.Data.Models
{
  public class Customer
  {
    public string CustomerName { get; set; }
    public CustomerType CustomerType { get; set; }
    public string CustomerUid { get; set; }
    public DateTime LastActionedUtc { get; set; }
  }
}
