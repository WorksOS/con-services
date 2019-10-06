using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  public class PointsListResult : ContractExecutionResult, IMasterDataModel
  {
    public List<List<WGSPoint>> PointsList { get; set; }
    
    public PointsListResult()
    { }

    public List<string> GetIdentifiers() => new List<string>();
  }
}
