using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Customer.Data.Models
{
  public class Customer
  {
    public Int32 CustomerID { get; set; }
    public string CustomerName { get; set; }
    public CustomerType fk_CustomerTypeID { get; set; }
    public string CustomerUID { get; set; }
    public DateTime LastCustomerUTC { get; set; }
  }
}
