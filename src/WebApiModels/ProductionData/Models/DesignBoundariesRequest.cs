using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Models
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
        projectId = projectId,
        tolerance = tolerance
      };
    }

    /// <summary>
    /// Create a sample instance of DesignBoundariesRequest class to display in Help documentation.
    /// </summary>
    public new static DesignBoundariesRequest HelpSample()
    {
      return new DesignBoundariesRequest
      {
        projectId = 123,
        tolerance = BOUNDARY_POINTS_INTERVAL,
      };
    }

    public const double BOUNDARY_POINTS_INTERVAL = 0.05;
  }
}
