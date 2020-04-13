using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class CustomerV1ListResult : ContractExecutionResult
  {
    public List<CustomerData> Customers { get; set; }
  }

  public class CustomerV1SingleResult : ContractExecutionResult
  {
    public CustomerData Customer;

    public CustomerV1SingleResult(CustomerData customer)
    {
      Customer = customer;
    }

    public CustomerData CustomerData { get { return Customer; } set { Customer = value; } }
  }
}
