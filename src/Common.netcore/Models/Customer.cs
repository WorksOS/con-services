using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Common.Models
{
  public class Customer
  {
    public string CustomerName { get; set; }
    public CustomerType CustomerType { get; set; }
    public string CustomerUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}