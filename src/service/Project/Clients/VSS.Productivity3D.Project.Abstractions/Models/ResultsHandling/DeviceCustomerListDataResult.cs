using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class DeviceCustomerListDataResult : BaseDataResult, IMasterDataModel
  {  
    /// <summary>
    /// Gets or sets the accounts associated with a device
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public List<DeviceCustomerSingleDataResult> DeviceCustomers { get; set; }

    public List<string> GetIdentifiers()
    {
      return DeviceCustomers?
               .SelectMany(p => p.GetIdentifiers())
               .Distinct()
               .ToList()
             ?? new List<string>();
    }
  }
}
