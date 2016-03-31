using System;

namespace VSS.UserCustomer.Data.Models
{
  public class UserCustomer
  {
    public Int32 UserCustomerID { get; set; }
    public string fk_UserUID { get; set; }
    public string fk_CustomerUID { get; set; }
    public Int32 fk_CustomerID { get; set; }
    public DateTime LastUserUTC { get; set; }
  }
}
