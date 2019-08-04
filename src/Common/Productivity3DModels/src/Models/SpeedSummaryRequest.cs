using System;

namespace VSS.Productivity3D.Models.Models
{
  public class SpeedSummaryRequest : TRexBaseRequest
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private SpeedSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public SpeedSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      MachineSpeedTarget machineSpeedTarget,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(machineSpeedTarget: machineSpeedTarget);
      LiftSettings = liftSettings;
    }
  }
}
