using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class ProjectExtentsResult : ContractExecutionResult
  {
    /// <summary>
    /// Minimum latitude of the production data extents in degrees
    /// </summary>
    public double minLat;
    /// <summary>
    /// Minimum longitude of the production data extents in degrees
    /// </summary>
    public double minLng;
    /// <summary>
    /// Maximum latitude of the production data extents in degrees
    /// </summary>
    public double maxLat;
    /// <summary>
    /// Maximum longitude of the production data extents in degrees
    /// </summary>
    public double maxLng;
  }
}
