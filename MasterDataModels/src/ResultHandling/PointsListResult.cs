using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.ResultHandling
{
  public class PointsListResult : ContractExecutionResult
  {
    public List<List<WGSPoint>> PointsList { get; set; }
  }
}
