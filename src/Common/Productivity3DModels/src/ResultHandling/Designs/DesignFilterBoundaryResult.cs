using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling.Designs
{
  /// <summary>
  /// Design filter boundary as list of points.
  /// </summary>
  public class DesignFilterBoundaryResult : ContractExecutionResult, IMasterDataModel
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

    public List<string> GetIdentifiers() => new List<string>();
  }
}
