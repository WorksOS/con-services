using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models.Designs
{
  /// <summary>
  /// The representation of a design boundary request.
  /// </summary>
  public class TRexDesignBoundariesRequest : ProjectID
  {
    /// <summary>
    /// The unique identifier of the design surface to to get boundaries from.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid DesignUid { get; private set; }

    /// <summary>
    /// The design file name.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public string FileName { get; private set; }

    /// <summary>
    /// Boundary points interval.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Tolerance { get; private set; }

    public TRexDesignBoundariesRequest(Guid projectUid, Guid designUid, string fileName, double tolerance)
    {
      ProjectUid = projectUid;
      DesignUid = designUid;
      FileName = fileName;
      Tolerance = tolerance;
    }
  }
}
