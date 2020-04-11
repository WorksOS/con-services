using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class CustomerV1DeviceLicenseResult : ContractExecutionResult
  {
    private int _totalLicenses;

    public CustomerV1DeviceLicenseResult(int totalLicenses)
    {
      this._totalLicenses = totalLicenses;
    }
  }
}
