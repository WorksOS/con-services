using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The representation of a summary volumes request
  /// </summary>
  public class DesignProfileRequest : ProjectID
  {
    /// <summary>
    /// The unique identifier of the design surface to be used as design the profile is computed across
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? DesignUid { get; private set; }

    /// <summary>
    /// Sets the StartX location of the profile line in meters
    /// </summary>
    /// <value>
    /// The start X.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? StartX { get; private set; }

    /// <summary>
    /// Sets the StartY location of the profile line in meters
    /// </summary>
    /// <value>
    /// The StartY.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? StartY { get; private set; }

    /// <summary>
    /// Sets the EndX location of the profile line in meters
    /// </summary>
    /// <value>
    /// The end X.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? EndX { get; private set; }

    /// <summary>
    /// Sets the StartY location of the profile line in meters
    /// </summary>
    /// <value>
    /// The EndY.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? EndY { get; private set; }

    /// <summary>
    /// Sets the fill tolerance to calculate Summary Volumes in meters
    /// </summary>
    /// <value>
    /// The cut tolerance.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? FillTolerance { get; private set; }

    /// <summary>
    /// Prevents a default instance of the <see cref="DesignProfileRequest"/> class from being created.
    /// </summary>
    private DesignProfileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    public DesignProfileRequest(
      Guid? projectUid, double? startX, double? startY, double? endX, double? endY)
    {
      ProjectUid = projectUid;
      StartX = startX;
      StartY = startY;
      EndX = endX;
      EndY = endY;
    }
  }
}
