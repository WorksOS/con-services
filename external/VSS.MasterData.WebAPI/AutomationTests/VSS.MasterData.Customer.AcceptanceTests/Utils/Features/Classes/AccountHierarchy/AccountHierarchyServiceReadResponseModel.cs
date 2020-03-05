using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchy
{
  public class Child
  {
    public string CustomerUID { get; set; }
    public string Name { get; set; }
    public string CustomerType { get; set; }
    public List<object> Children { get; set; }
  }

  public class Customer
  {
    public string CustomerUID { get; set; }
    public string Name { get; set; }
    public string CustomerType { get; set; }
    public List<Child> Children { get; set; }
  }

  public class AccountHierarchyServiceReadResponseModel
  {
    public string UserUID { get; set; }
    public List<Customer> Customers { get; set; }
  }
}
