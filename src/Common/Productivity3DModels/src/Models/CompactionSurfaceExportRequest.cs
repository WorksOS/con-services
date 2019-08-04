using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request export data.
  /// </summary>
  public class CompactionSurfaceExportRequest : CompactionExportRequest
  {
    /// <summary>
    /// Sets the tolerance to calculate TIN surfaces.
    /// </summary>
    /// <remarks>
    /// The value should be in meters.
    /// </remarks>
    [JsonProperty(Required = Required.Default)]
    public double? Tolerance { get; private set; }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionSurfaceExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName,
      double tolerance,
      OverridingTargets overrides,
      LiftSettings liftSettings
      ) : base(projectUid, filter, fileName, overrides, liftSettings)
    {
      Tolerance = tolerance;
    }
  }
}
