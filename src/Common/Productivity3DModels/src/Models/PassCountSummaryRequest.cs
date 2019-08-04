using System;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request Pass Count summary.
  /// </summary>
  public class PassCountSummaryRequest : TRexBaseRequest
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private PassCountSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public PassCountSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      TargetPassCountRange overridingTargetPassCountRange,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(overridingTargetPassCountRange: overridingTargetPassCountRange);
      LiftSettings = liftSettings;
    }
  }
}
