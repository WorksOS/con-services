using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models.Profiling
{
  /// <summary>
  /// The representation of a profile request.
  /// </summary>
  public class BaseProfileDataRequest : TRexBaseRequest
  {
    /// <summary>
    /// The unique identifier of the design surface to be used for cut-fill for production data profiles
    /// or filter to design or design to filter for volumes profiles
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? ReferenceDesignUid { get; private set; }

    /// <summary>
    /// The offset if the design surface is a reference surface
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double? ReferenceDesignOffset { get; private set; }

    /// <summary>
    ///  Start grid position x or start lon
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public double StartX { get; private set; }

    /// <summary>
    ///  Start grid position y or start lat
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public double StartY { get; private set; }

    /// <summary>
    ///  End grid position y or end lat
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public double EndX { get; private set; }

    /// <summary>
    ///  End grid position y or end lat
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public double EndY { get; private set; }

    /// <summary>
    ///  Are positions grid or latlon
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public bool PositionsAreGrid { get; private set; }
    /// <summary>
    /// Default public constructor.
    /// </summary>
    public BaseProfileDataRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public BaseProfileDataRequest(
      Guid projectUid,
      Guid? referenceDesignUid,
      double? referenceDesignOffset,
      bool positionsAreGrid,
      double startX,
      double startY,
      double endX,
      double endY,
      OverridingTargets overrides,
      LiftSettings liftSettings,
      FilterResult filter)
    {
      ProjectUid = projectUid;
      ReferenceDesignUid = referenceDesignUid;
      ReferenceDesignOffset = referenceDesignOffset;
      PositionsAreGrid = positionsAreGrid;
      StartX = startX;
      StartY = startY;
      EndX = endX;
      EndY = endY;
      Overrides = overrides;
      LiftSettings = liftSettings;
      Filter = filter;
    }

  }
}
