using Newtonsoft.Json;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// A representation of a design boundaries request.
  /// </summary>
  /// 
  public class DesignBoundariesRequest : ProjectID
  {
    [JsonProperty(PropertyName = "tolerance", Required = Required.Default)]
    public double tolerance { get; protected set; }

    /// <summary>
    /// Creates an instance of DesignBoundariesRequest class to display in Help documentation.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    /// 
    public static DesignBoundariesRequest CreateDesignBoundariesRequest(long projectId, double tolerance)
    {
      return new DesignBoundariesRequest
      {
        ProjectId = projectId,
        tolerance = tolerance
      };
    }
    
    public const double BOUNDARY_POINTS_INTERVAL = 1.00;
  }
}
