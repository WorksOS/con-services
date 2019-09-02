using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// A representation of a design boundaries request.
  /// </summary>
  /// 
  public class DesignBoundariesRequest : ProjectID
  {
    [JsonProperty(PropertyName = "tolerance", Required = Required.Default)]
    public double Tolerance { get; protected set; }

    /// <summary>
    /// Creates an instance of DesignBoundariesRequest class to display in Help documentation.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectUid"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public DesignBoundariesRequest(long projectId, Guid? projectUid, double tolerance)
    {
      ProjectId = projectId;
      ProjectUid = projectUid;
      Tolerance = tolerance;
    }
    
    public const double BOUNDARY_POINTS_INTERVAL = 0.0;
  }
}
