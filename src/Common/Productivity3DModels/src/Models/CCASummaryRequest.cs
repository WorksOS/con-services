using System;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used for a CCA Summary request.
  /// </summary>
  public class CCASummaryRequest : TRexBaseRequest
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CCASummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CCASummaryRequest(
      Guid projectUid,
      FilterResult filter,
      OverridingTargets overrides,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

  }
}
