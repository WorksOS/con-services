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
    public Guid? DesignUid { get; private set; }

    ///// <summary>
    ///// The interval the stationing text entities/labels are displayed with.
    ///// </summary>
    //[JsonProperty(Required = Required.Default)]
    //public double LabellingInterval { get; private set; }

    /// <summary>
    /// Boundary points interval.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Tolerance { get; private set; }

    public TRexDesignBoundariesRequest(Guid projectUid, Guid? designUid, double tolerance)
    {
      ProjectUid = projectUid;
      DesignUid = designUid;
      Tolerance = tolerance;
    }
  }
}
