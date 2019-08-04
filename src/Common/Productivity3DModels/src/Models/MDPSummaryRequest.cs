using System;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request MDP summary.
  /// </summary>
  public class MDPSummaryRequest : TRexBaseRequest
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MDPSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public MDPSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      short mdpTarget,
      bool overrideTargetMDP,
      double maxMDPPercent,
      double minMDPPercent,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(mdpTarget: mdpTarget, overrideTargetMDP: overrideTargetMDP, maxMDPPercent: maxMDPPercent, minMDPPercent: minMDPPercent);
      LiftSettings = liftSettings;
    }
  }
}
