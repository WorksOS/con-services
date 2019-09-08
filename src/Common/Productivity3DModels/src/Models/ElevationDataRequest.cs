using System;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request Elevation statistics.
  /// </summary>
  public class ElevationDataRequest : TRexBaseRequest
  {
    /// <summary>
    /// Default private constructor
    /// </summary>
    private ElevationDataRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public ElevationDataRequest(
      Guid projectUid, 
      FilterResult filter,
      OverridingTargets overrides,
      LiftSettings liftSettings)
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

  }
}
