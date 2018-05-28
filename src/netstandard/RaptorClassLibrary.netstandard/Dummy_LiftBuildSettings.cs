using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Types;

namespace VSS.TRex
{
  /// <summary>
  /// A dummy class representing the Raptor lift build settigns schema. This is a large collection of semi-related configuration
  /// elements thet need significant re-design for TRes. This dummy class is a place holder to permit initial conversion of Raptor
  /// code to TRex pending those structural refactorings.
  /// </summary>
    public static class Dummy_LiftBuildSettings
  {
    public static bool OverrideMachineThickness = false;
    public static LiftThicknessType LiftThicknessType = LiftThicknessType.Compacted;
    public static double OverridingLiftThickness = 0.0;
    public static bool OverrideMachineCCV = false;
    public static short OverridingMachineCCV = 0;
    public static CCVRangePercentage CCVRange = new CCVRangePercentage();
    public static byte CCVSummaryTypes = 0;
    public static bool CCVSummarizeTopLayerOnly = false;
    public static double FirstPassThickness = 0.0;

    public static bool OverrideMachineMDP= false;
    public static short OverridingMachineMDP = 0;
    public static MDPRangePercentage MDPRange = new MDPRangePercentage();
    public static byte MDPsummaryTypes = 0;
    public static bool MDPSummarizeTopLayerOnly = false;

    public static LayerMethod LiftDetectionType = LayerMethod.AutoMapReset;
    public static TargetPassCountRange OverridingTargetPassCountRange = new TargetPassCountRange();
    public static bool OverrideTargetPassCount = false;
    public static TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord();
    public static bool OverrideTemperatureWarningLevels = false;

    public static bool IncludeSuperseded = false;

      //Parameters controlling TargetLiftThcknessSummary overlay
      public static double TargetLiftThickness = 0.0;
    public static double AboveToleranceLiftThickness = 0.0;
    public static double BelowToleranceLiftThickness = 0.0;

      // Boundaries extending above/below a cell pass constituting the dead band
    public static double DeadBandLowerBoundary = 0.0;
    public static double DeadBandUpperBoundary = 0.0;

      // Setup target speeds here for SpeedSummary request

    public static ushort TargetMaxMachineSpeed = 0;
    public static ushort TargetMinMachineSpeed = 0;

      public static int CCATolerance = 0;  // How many extra passes is OK before overcompaction is set
    }
}
