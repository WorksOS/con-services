using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling.Designs
{
  /// <summary>
  /// Design filter boundary as list of points.
  /// </summary>
  public class DesignFilterBoundaryResult : ContractExecutionResult
  {
    /// <summary>
    /// Design filter boundary as list of points.
    /// </summary>
    public List<WGSPoint> Fence { get; private set; }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    /// <param name="fence"></param>
    public DesignFilterBoundaryResult(List<WGSPoint> fence)
    {
      Fence = fence;
    }
  }
}
