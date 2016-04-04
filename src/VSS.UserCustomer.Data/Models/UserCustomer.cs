using System;

namespace VSS.UserCustomer.Data.Models
{
  public class UserCustomer
  {
    public string fk_UserUID { get; set; }
    public string fk_CustomerUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
