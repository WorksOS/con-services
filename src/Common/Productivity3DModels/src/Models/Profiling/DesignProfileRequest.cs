using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.Models.Models.Profiling
{
  /// <summary>
  /// The representation of a design profile request.
  /// </summary>
  public class DesignProfileRequest : ProjectID
  {
    /// <summary>
    /// The unique identifier of the design surface to be used as design the profile is computed across
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public Guid DesignUid { get; private set; }

    /// <summary>
    /// The offset from the design for a reference surface
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public double Offset { get; private set; }

    /// <summary>
    /// Sets the StartX location of the profile line in meters for grid coordinates and in radians for lat/lng coordinates
    /// </summary>
    /// <value>
    /// The start X.
    /// </value>
    [JsonProperty(Required = Required.Always)]
    public double StartX { get; private set; }

    /// <summary>
    /// Sets the StartY location of the profile line in meters for grid coordinates and in radians for lat/lng coordinates
    /// </summary>
    /// <value>
    /// The StartY.
    /// </value>
    [JsonProperty(Required = Required.Always)]
    public double StartY { get; private set; }

    /// <summary>
    /// Sets the EndX location of the profile line in meters for grid coordinates and in radians for lat/lng coordinates
    /// </summary>
    /// <value>
    /// The end X.
    /// </value>
    [JsonProperty(Required = Required.Always)]
    public double EndX { get; private set; }

    /// <summary>
    /// Sets the StartY location of the profile line in meters for grid coordinates and in radians for lat/lng coordinates
    /// </summary>
    /// <value>
    /// The EndY.
    /// </value>
    [JsonProperty(Required = Required.Always)]
    public double EndY { get; private set; }

    /// <summary>
    ///  Are positions grid or lat/lng
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public bool PositionsAreGrid { get; private set; }

    /// <summary>
    /// Prevents a default instance of the <see cref="DesignProfileRequest"/> class from being created.
    /// </summary>
    private DesignProfileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public DesignProfileRequest(Guid projectUid, Guid designUid, double offset, double startX, double startY, double endX, double endY, bool positionsAreGrid)
    {
      ProjectUid = projectUid;
      DesignUid = designUid;
      Offset = offset;
      StartX = startX;
      StartY = startY;
      EndX = endX;
      EndY = endY;
      PositionsAreGrid = positionsAreGrid;
    }
  }
}
