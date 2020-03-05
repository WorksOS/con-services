using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchyWebAPI
{
  public class CustomerDetailsModel
  {
   public string CustomerName { get; set; }
   public string CustomerType { get; set; }
   public string DealerNetwork { get; set; }
   public string NetworkDealerCode { get; set; }
   public string NetworkCustomerCode { get; set; }
   public string DealerAccountCode { get; set; }
   public Guid CustomerUID { get; set; }

 
  }
  public class CustomerUserAssociationModel
  {
    public Guid CustomerUID { get; set; }
    public Guid UserUID { get; set; }
  }
}
