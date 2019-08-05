using VSS.TRex.Common.Models;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Types.Types;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Tracks the set of machine event sources that a query requires to satisfy its function
  /// </summary>
  public class FilteredValuePopulationControl : IFilteredValuePopulationControl
  {
    public bool WantsTargetCCVValues { get; set; }
    public bool WantsTargetPassCountValues { get; set; }
    public bool WantsTargetLiftThicknessValues { get; set; }
    public bool WantsEventDesignNameValues { get; set; }
    public bool WantsEventVibrationStateValues { get; set; }
    public bool WantsEventAutoVibrationStateValues { get; set; }
    public bool WantsEventICFlagsValues { get; set; }
    public bool WantsEventMachineGearValues { get; set; }
    public bool WantsEventMachineCompactionRMVJumpThreshold { get; set; }
    public bool WantsEventMachineAutomaticsValues { get; set; }
    public bool WantsEventMapResetValues { get; set; }
    public bool WantsEventElevationMappingModeValues { get; set; }
    public bool WantsEventInAvoidZoneStateValues { get; set; }
    public bool WantsEventGPSAccuracyValues { get; set; }
    public bool WantsEventPositioningTechValues { get; set; }
    public bool WantsTempWarningLevelMinValues { get; set; }
    public bool WantsTempWarningLevelMaxValues { get; set; }
    public bool WantsTargetMDPValues { get; set; }
    public bool WantsLayerIDValues { get; set; }
    public bool WantsTargetCCAValues { get; set; }
    public bool WantsEventGPSModeValues { get; set; }

    /// <summary>
    /// Determines if any of the population flags are set
    /// </summary>
    /// <returns></returns>
    public bool AnySet()
    {
      return WantsTargetCCVValues ||
             WantsTargetPassCountValues ||
             WantsTargetLiftThicknessValues ||
             WantsEventDesignNameValues ||
             WantsEventVibrationStateValues ||
             WantsEventAutoVibrationStateValues ||
             WantsEventICFlagsValues ||
             WantsEventMachineGearValues ||
             WantsEventMachineCompactionRMVJumpThreshold ||
             WantsEventMachineAutomaticsValues ||
             WantsEventMapResetValues ||
             WantsEventElevationMappingModeValues ||
             WantsEventInAvoidZoneStateValues ||
             WantsEventGPSAccuracyValues ||
             WantsEventPositioningTechValues ||
             WantsTempWarningLevelMinValues ||
             WantsTempWarningLevelMaxValues ||
             WantsTargetMDPValues ||
             WantsLayerIDValues ||
             WantsTargetCCAValues ||
             WantsEventGPSModeValues;
    }

    /// <summary>
    /// Sets all event population flags to false
    /// </summary>
    public void Clear()
    {
      WantsTargetCCVValues = false;
      WantsTargetPassCountValues = false;
      WantsTargetLiftThicknessValues = false;
      WantsEventDesignNameValues = false;
      WantsEventVibrationStateValues = false;
      WantsEventAutoVibrationStateValues = false;
      WantsEventICFlagsValues = false;
      WantsEventMachineGearValues = false;
      WantsEventMachineCompactionRMVJumpThreshold = false;
      WantsEventMachineAutomaticsValues = false;
      WantsEventMapResetValues = false;
      WantsEventElevationMappingModeValues = false;
      WantsEventInAvoidZoneStateValues = false;
      WantsEventGPSAccuracyValues = false;
      WantsEventPositioningTechValues = false;
      WantsTempWarningLevelMinValues = false;
      WantsTempWarningLevelMaxValues = false;
      WantsTargetMDPValues = false;
      WantsLayerIDValues = false;
      WantsTargetCCAValues = false;
      WantsEventGPSModeValues = false;
    }

    /// <summary>
    /// Sets all event population flags to true
    /// </summary>
    public void Fill()
    {
      WantsTargetCCVValues = true;
      WantsTargetPassCountValues = true;
      WantsTargetLiftThicknessValues = true;
      WantsEventDesignNameValues = true;
      WantsEventVibrationStateValues = true;
      WantsEventAutoVibrationStateValues = true;
      WantsEventICFlagsValues = true;
      WantsEventMachineGearValues = true;
      WantsEventMachineCompactionRMVJumpThreshold = true;
      WantsEventMachineAutomaticsValues = true;
      WantsEventMapResetValues = true;
      WantsEventElevationMappingModeValues = true;
      WantsEventInAvoidZoneStateValues = true;
      WantsEventGPSAccuracyValues = true;
      WantsEventPositioningTechValues = true;
      WantsTempWarningLevelMinValues = true;
      WantsTempWarningLevelMaxValues = true;
      WantsTargetMDPValues = true;
      WantsLayerIDValues = true;
      WantsTargetCCAValues = true;
      WantsEventGPSModeValues = true;
    }

    /// <summary>
    /// Converts the set of event population flags into a bit-flagged integer
    /// </summary>
    /// <returns></returns>
    public uint GetFlags()
    {
      return (WantsTargetCCVValues ? (uint)PopulationControlFlags.WantsTargetCCVValues : 0) |
             (WantsTargetPassCountValues ? (uint)PopulationControlFlags.WantsTargetPassCountValues : 0) |
             (WantsTargetLiftThicknessValues ? (uint)PopulationControlFlags.WantsTargetThicknessValues : 0)  |
             (WantsEventDesignNameValues ? (uint)PopulationControlFlags.WantsEventDesignNameValues : 0) |
             (WantsEventVibrationStateValues ? (uint)PopulationControlFlags.WantsEventVibrationStateValues : 0) |
             (WantsEventAutoVibrationStateValues ? (uint)PopulationControlFlags.WantsEventAutoVibrationStateValues : 0) |
             (WantsEventICFlagsValues ? (uint)PopulationControlFlags.WantsEventICFlagsValues : 0) |
             (WantsEventMachineGearValues ? (uint)PopulationControlFlags.WantsEventMachineGearValues : 0) |
             (WantsEventMachineCompactionRMVJumpThreshold ? (uint)PopulationControlFlags.WantsEventMachineCompactionRMVJumpThreshold : 0) |
             (WantsEventMachineAutomaticsValues ? (uint)PopulationControlFlags.WantsEventMachineAutomaticsValues : 0) |
             (WantsEventMapResetValues ? (uint)PopulationControlFlags.WantsEventMapResetValues : 0) |
             (WantsEventElevationMappingModeValues ? (uint)PopulationControlFlags.WantsEventElevationMappingModeValues : 0) |
             (WantsEventInAvoidZoneStateValues ? (uint)PopulationControlFlags.WantsEventInAvoidZoneStateValues : 0) |
             (WantsEventGPSAccuracyValues ? (uint)PopulationControlFlags.WantsEventGPSAccuracyValues : 0) |
             (WantsEventPositioningTechValues ? (uint)PopulationControlFlags.WantsEventPositioningTechValues : 0) |
             (WantsTempWarningLevelMinValues ? (uint)PopulationControlFlags.WantsTempWarningLevelMinValues : 0) |
             (WantsTempWarningLevelMaxValues ? (uint)PopulationControlFlags.WantsTempWarningLevelMaxValues : 0) |
             (WantsTargetMDPValues ? (uint)PopulationControlFlags.WantsTargetMDPValues : 0) |
             (WantsLayerIDValues ? (uint)PopulationControlFlags.WantsLayerIDValues : 0) |
             (WantsTargetCCAValues ? (uint)PopulationControlFlags.WantsTargetCCAValues : 0) |
             (WantsEventGPSModeValues ? (uint)PopulationControlFlags.WantsEventGPSModeValues : 0);
    }

    /// <summary>
    /// Converts a bit-flagged integer into the set of event population flags
    /// </summary>
    /// <param name="flags"></param>
    public void SetFromFlags(uint flags)
    {
      WantsTargetCCVValues = (flags & (uint)PopulationControlFlags.WantsTargetCCVValues) != 0;
      WantsTargetPassCountValues = (flags & (uint)PopulationControlFlags.WantsTargetPassCountValues) != 0;
      WantsTargetLiftThicknessValues = (flags & (uint)PopulationControlFlags.WantsTargetThicknessValues) != 0;
      WantsEventDesignNameValues = (flags & (uint)PopulationControlFlags.WantsEventDesignNameValues) != 0;
      WantsEventVibrationStateValues = (flags & (uint)PopulationControlFlags.WantsEventVibrationStateValues) != 0;
      WantsEventAutoVibrationStateValues = (flags & (uint)PopulationControlFlags.WantsEventAutoVibrationStateValues) != 0;
      WantsEventICFlagsValues = (flags & (uint)PopulationControlFlags.WantsEventICFlagsValues) != 0;
      WantsEventMachineGearValues = (flags & (uint)PopulationControlFlags.WantsEventMachineGearValues) != 0;
      WantsEventMachineCompactionRMVJumpThreshold = (flags & (uint)PopulationControlFlags.WantsEventMachineCompactionRMVJumpThreshold) != 0;
      WantsEventMachineAutomaticsValues = (flags & (uint)PopulationControlFlags.WantsEventMachineAutomaticsValues) != 0;
      WantsEventMapResetValues = (flags & (uint)PopulationControlFlags.WantsEventMapResetValues) != 0;
      WantsEventElevationMappingModeValues = (flags & (uint)PopulationControlFlags.WantsEventElevationMappingModeValues) != 0;
      WantsEventInAvoidZoneStateValues = (flags & (uint)PopulationControlFlags.WantsEventInAvoidZoneStateValues) != 0;
      WantsEventGPSAccuracyValues = (flags & (uint)PopulationControlFlags.WantsEventGPSAccuracyValues) != 0;
      WantsEventPositioningTechValues = (flags & (uint)PopulationControlFlags.WantsEventPositioningTechValues) != 0;
      WantsTempWarningLevelMinValues = (flags & (uint)PopulationControlFlags.WantsTempWarningLevelMinValues) != 0;
      WantsTempWarningLevelMaxValues = (flags & (uint)PopulationControlFlags.WantsTempWarningLevelMaxValues) != 0;
      WantsTargetMDPValues = (flags & (uint)PopulationControlFlags.WantsTargetMDPValues) != 0;
      WantsLayerIDValues = (flags & (uint)PopulationControlFlags.WantsLayerIDValues) != 0;
      WantsTargetCCAValues = (flags & (uint)PopulationControlFlags.WantsTargetCCAValues) != 0;
      WantsEventGPSModeValues = (flags & (uint)PopulationControlFlags.WantsEventGPSModeValues) != 0;
    }

    /// <summary>
    /// Calculates the a set of lift build setting flags from the required data type and other lift build settings
    /// </summary>
    public static void CalculateFlags(GridDataType ProfileTypeRequired,
      ILiftParameters liftParams,
      out bool compactionSummaryInLiftBuildSettings,
      out bool workInProgressSummaryInLiftBuildSettings,
      out bool thicknessInProgressInLiftBuildSettings)
    {
      if (ProfileTypeRequired == GridDataType.All || ProfileTypeRequired == GridDataType.CCV ||
          ProfileTypeRequired == GridDataType.CCVPercent)
      {
        compactionSummaryInLiftBuildSettings = liftParams.CCVSummaryTypes != CCVSummaryTypes.None; 
        workInProgressSummaryInLiftBuildSettings = compactionSummaryInLiftBuildSettings;
        thicknessInProgressInLiftBuildSettings = compactionSummaryInLiftBuildSettings;
      }
      else if (ProfileTypeRequired == GridDataType.All || ProfileTypeRequired == GridDataType.MDP ||
               ProfileTypeRequired == GridDataType.MDPPercent)
      {
        compactionSummaryInLiftBuildSettings = liftParams.MDPSummaryTypes != MDPSummaryTypes.None;
        workInProgressSummaryInLiftBuildSettings = compactionSummaryInLiftBuildSettings;
        thicknessInProgressInLiftBuildSettings = compactionSummaryInLiftBuildSettings;
      }

      if (ProfileTypeRequired == GridDataType.All || ProfileTypeRequired == GridDataType.CCA ||
          ProfileTypeRequired == GridDataType.CCAPercent)
      {
        compactionSummaryInLiftBuildSettings = true;
        workInProgressSummaryInLiftBuildSettings = compactionSummaryInLiftBuildSettings;
        thicknessInProgressInLiftBuildSettings = compactionSummaryInLiftBuildSettings;
      }
      else
      {
        compactionSummaryInLiftBuildSettings = false;
        workInProgressSummaryInLiftBuildSettings = false;
        thicknessInProgressInLiftBuildSettings = false;
      }
    }

    /// <summary>
    /// Prepares the set of event population control flags depending on the requested data type, filter, client grid
    /// and lift build related settings
    /// </summary>
    public void PreparePopulationControl(GridDataType profileTypeRequired,
      ILiftParameters liftParams,
      ICellPassAttributeFilter passFilter,
      PopulationControlFlags eventPopulationFlags)
    {
      CalculateFlags(profileTypeRequired, liftParams,
        out bool compactionSummaryInLiftBuildSettings, out bool workInProgressSummaryInLiftBuildSettings,
        out bool thicknessInProgressInLiftBuildSettings);

      Clear();

      WantsTargetPassCountValues =
        (eventPopulationFlags & PopulationControlFlags.WantsTargetPassCountValues) != 0;
      WantsEventAutoVibrationStateValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventAutoVibrationStateValues) != 0;
      WantsEventICFlagsValues = (eventPopulationFlags & PopulationControlFlags.WantsEventICFlagsValues) != 0;
      WantsEventMachineGearValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventMachineGearValues) != 0 ||
        passFilter.HasMachineDirectionFilter;
      WantsEventMachineCompactionRMVJumpThreshold = (eventPopulationFlags &
                                                     PopulationControlFlags
                                                       .WantsEventMachineCompactionRMVJumpThreshold) != 0;
      WantsTempWarningLevelMinValues =
        (eventPopulationFlags & PopulationControlFlags.WantsTempWarningLevelMinValues) != 0;
      WantsTempWarningLevelMaxValues =
        (eventPopulationFlags & PopulationControlFlags.WantsTempWarningLevelMaxValues) != 0;
      WantsEventMapResetValues = liftParams.LiftDetectionType == LiftDetectionType.MapReset || liftParams.LiftDetectionType == LiftDetectionType.AutoMapReset;
      WantsEventDesignNameValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventDesignNameValues) != 0 ||
        passFilter.HasDesignFilter ||
        WantsEventMapResetValues;
      WantsTargetCCVValues = (eventPopulationFlags & PopulationControlFlags.WantsTargetCCVValues) != 0 ||
                             compactionSummaryInLiftBuildSettings ||
                             workInProgressSummaryInLiftBuildSettings;
      WantsTargetMDPValues = (eventPopulationFlags & PopulationControlFlags.WantsTargetMDPValues) != 0 ||
                             compactionSummaryInLiftBuildSettings ||
                             workInProgressSummaryInLiftBuildSettings;
      WantsTargetCCAValues = (eventPopulationFlags & PopulationControlFlags.WantsTargetCCAValues) != 0 ||
                             compactionSummaryInLiftBuildSettings ||
                             workInProgressSummaryInLiftBuildSettings;
      WantsTargetLiftThicknessValues =
        (eventPopulationFlags & PopulationControlFlags.WantsTargetThicknessValues) != 0 
          || thicknessInProgressInLiftBuildSettings
          || workInProgressSummaryInLiftBuildSettings
          || liftParams.LiftDetectionType == LiftDetectionType.Automatic 
          || liftParams.LiftDetectionType == LiftDetectionType.AutoMapReset; 
      WantsEventVibrationStateValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventVibrationStateValues) != 0 ||
        passFilter.HasVibeStateFilter ||
        (profileTypeRequired == GridDataType.All ||
         profileTypeRequired == GridDataType.CCV ||
         profileTypeRequired == GridDataType.CCVPercent ||
         profileTypeRequired == GridDataType.RMV ||
         profileTypeRequired == GridDataType.Frequency ||
         profileTypeRequired == GridDataType.Amplitude);
      WantsEventElevationMappingModeValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventElevationMappingModeValues) != 0 ||
        passFilter.HasElevationMappingModeFilter;
      WantsEventInAvoidZoneStateValues =
        false; // (eventPopulationFlags & PopulationControlFlags.WantsInAvoidZoneStateValues) != 0 || passFilter.HasAvoidZoneStateFilter;
      WantsEventGPSAccuracyValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventGPSAccuracyValues) != 0 ||
        passFilter.HasGPSAccuracyFilter || passFilter.HasGPSToleranceFilter;
      WantsEventPositioningTechValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventPositioningTechValues) != 0 ||
        passFilter.HasPositioningTechFilter;
      WantsEventMachineAutomaticsValues =
        (eventPopulationFlags & PopulationControlFlags.WantsEventMachineAutomaticsValues) != 0 ||
        passFilter.HasGCSGuidanceModeFilter;
      WantsLayerIDValues = profileTypeRequired == GridDataType.CellProfile ||
                           profileTypeRequired == GridDataType.CellPasses ||
                           passFilter.HasLayerIDFilter ||
                           liftParams.LiftDetectionType == LiftDetectionType.Tagfile ||
                           liftParams.LiftDetectionType == LiftDetectionType.MapReset ||
                           liftParams.LiftDetectionType == LiftDetectionType.AutoMapReset;
    }

    /// <summary>
    /// Prepares the set of event population control flags depending on the requested data type and filter
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="passFilter"></param>
    public void PreparePopulationControl(GridDataType profileTypeRequired,
      ILiftParameters liftParams,
      ICellPassAttributeFilter passFilter)
    {
      CalculateFlags(profileTypeRequired, liftParams,
        out bool compactionSummaryInLiftBuildSettings, out bool workInProgressSummaryInLiftBuildSettings,
        out bool thicknessInProgressInLiftBuildSettings);

      Clear();

      WantsTargetPassCountValues = profileTypeRequired == GridDataType.All;

      WantsTempWarningLevelMinValues = profileTypeRequired == GridDataType.All;
      WantsTempWarningLevelMaxValues = profileTypeRequired == GridDataType.All;

      WantsEventMachineGearValues = passFilter.HasMachineDirectionFilter;
      WantsEventMapResetValues = liftParams.LiftDetectionType == LiftDetectionType.MapReset || 
                                 liftParams.LiftDetectionType == LiftDetectionType.AutoMapReset;
      WantsEventDesignNameValues = passFilter.HasDesignFilter || WantsEventMapResetValues;

      WantsTargetCCVValues = compactionSummaryInLiftBuildSettings || workInProgressSummaryInLiftBuildSettings;
      WantsTargetMDPValues = compactionSummaryInLiftBuildSettings || workInProgressSummaryInLiftBuildSettings;
      WantsTargetCCAValues = compactionSummaryInLiftBuildSettings || workInProgressSummaryInLiftBuildSettings;

      WantsTargetLiftThicknessValues = workInProgressSummaryInLiftBuildSettings || 
                                       thicknessInProgressInLiftBuildSettings ||
                                       liftParams.LiftDetectionType == LiftDetectionType.Automatic || 
                                       liftParams.LiftDetectionType == LiftDetectionType.AutoMapReset;

      WantsEventVibrationStateValues = passFilter.HasVibeStateFilter ||
                                       (profileTypeRequired == GridDataType.All ||
                                        profileTypeRequired == GridDataType.CCV ||
                                        profileTypeRequired == GridDataType.CCVPercent ||
                                        profileTypeRequired == GridDataType.RMV ||
                                        profileTypeRequired == GridDataType.Frequency ||
                                        profileTypeRequired == GridDataType.Amplitude);

      WantsEventElevationMappingModeValues = passFilter.HasElevationMappingModeFilter;
      WantsEventInAvoidZoneStateValues = false; // todo passFilter.HasAvoidZoneStateFilter;
      WantsEventGPSAccuracyValues = passFilter.HasGPSAccuracyFilter || passFilter.HasGPSToleranceFilter;
      WantsEventPositioningTechValues = passFilter.HasPositioningTechFilter;
      WantsEventMachineAutomaticsValues = passFilter.HasGCSGuidanceModeFilter;
      WantsLayerIDValues = profileTypeRequired == GridDataType.CellProfile ||
                          profileTypeRequired == GridDataType.CellPasses ||
                          passFilter.HasLayerIDFilter ||
                          liftParams.LiftDetectionType == LiftDetectionType.Tagfile ||
                          liftParams.LiftDetectionType == LiftDetectionType.MapReset ||
                          liftParams.LiftDetectionType == LiftDetectionType.AutoMapReset;
    }
  }
}

