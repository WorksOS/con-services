using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  public class AlignmentPointsResult : ContractExecutionResult
  {
    public List<WGSPoint> AlignmentPoints { get; set; }
  }
}
