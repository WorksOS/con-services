using System;

namespace VSS.Customer.Data.Models
{
  public class CustomerUser
  {
    public string UserUID { get; set; }
    public string CustomerUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
