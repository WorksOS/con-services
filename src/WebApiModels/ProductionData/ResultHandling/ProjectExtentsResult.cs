using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// ProjectExtentsResult
  /// </summary>
  public class ProjectExtentsResult : ContractExecutionResult
  {
    /// <summary>
    /// BoundingBox3DGrid
    /// </summary>
    public BoundingBox3DGrid ProjectExtents { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProjectExtentsResult()
    { }
    
    /// <summary>
    /// ProjectExtentsResult create instance
    /// </summary>
    public static ProjectExtentsResult CreateProjectExtentsResult(BoundingBox3DGrid convertedExtents)
    {
      return new ProjectExtentsResult
      {
        ProjectExtents = convertedExtents
      };
    }
  }
}
