using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchyWebAPI
{
  public class CustomerDetails
  {
    public string CustomerUID { get; set; }
    public string Name { get; set; }
    public string CustomerType { get; set; }
    public string NetworkDealerCode { get; set; }
    public string NetworkCustomerCode { get; set; }
    public string DealerAccountCode { get; set; }
    public string DisplayName { get; set; }
    public List<CustomerDetails> Children { get; set; }
  }
  public class AccountHierarchyResponse
  {
    public string UserUID { get; set; }
    public List<CustomerDetails> Customers { get; set; }
  }
}
