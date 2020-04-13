using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class CustomerV1DeviceLicenseResult : ContractExecutionResult
  {
    public int TotalLicenses;

    public CustomerV1DeviceLicenseResult(int totalLicenses)
    {
      this.TotalLicenses = totalLicenses;
    }
  }
}
