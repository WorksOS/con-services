using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Customer.Data.Models
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
