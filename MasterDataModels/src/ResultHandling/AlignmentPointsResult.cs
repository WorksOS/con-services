using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.ResultHandling
{
  public class AlignmentPointsResult : ContractExecutionResult
  {
    public List<WGSPoint> AlignmentPoints { get; set; }
  }
}
