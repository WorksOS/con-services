using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// The representation of a design boundary request.
  /// </summary>
  public class DesignBoundariesRequest : DesignDataRequest
  {
    /// <summary>
    /// Boundary points interval.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Tolerance { get; private set; }

    public DesignBoundariesRequest(Guid projectUid, Guid designUid, string fileName, double tolerance) : 
      base (projectUid, designUid, fileName)
    {
      Tolerance = tolerance;
    }
  }
}
