using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class DeviceDataSingleResult : BaseDataResult, IMasterDataModel
  {  
    public DeviceData DeviceDescriptor { get; set; }

    public List<string> GetIdentifiers() => DeviceDescriptor?.GetIdentifiers() ?? new List<string>();
  }
}
