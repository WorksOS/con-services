using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class CustomerV1ListResult : ContractExecutionResult
  {
    public List<CustomerData> customers { get; set; }
  }

  public class CustomerV1SingleResult : ContractExecutionResult
  {
    private CustomerData _customer;

    public CustomerV1SingleResult(CustomerData customer)
    {
      this._customer = customer;
    }

    public CustomerData Customer { get { return _customer; } set { _customer = value; } }
  }

  public class CustomerV1DeviceLicenseResult : ContractExecutionResult
  {
    private int _totalLicenses;

    public CustomerV1DeviceLicenseResult(int totalLicenses)
    {
      this._totalLicenses = totalLicenses;
    }
  }
}
