using System;

namespace Common.Models
{
  public class CustomerUser
  {
    public string UserUID { get; set; }
    public string CustomerUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
