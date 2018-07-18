using System;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Tracks the set of machine evnt sources that a query requires to satisfy its function
  /// </summary>
  public class FilteredValuePopulationControl
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
    public bool WantsEventMinElevMappingValues { get; set; }
    public bool WantsEventInAvoidZoneStateValues { get; set; }
    public bool WantsEventGPSAccuracyValues { get; set; }
    public bool WantsEventPositioningTechValues { get; set; }
    public bool WantsTempWarningLevelMinValues { get; set; }
    public bool WantsTempWarningLevelMaxValues { get; set; }
    public bool WantsTargetMDPValues { get; set; }
    public bool WantsLayerIDValues { get; set; }
    public bool WantsTargetCCAValues { get; set; }

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
             WantsEventMinElevMappingValues ||
             WantsEventInAvoidZoneStateValues ||
             WantsEventGPSAccuracyValues ||
             WantsEventPositioningTechValues ||
             WantsTempWarningLevelMinValues ||
             WantsTempWarningLevelMaxValues ||
             WantsTargetMDPValues ||
             WantsLayerIDValues ||
             WantsTargetCCAValues;
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
      WantsEventMinElevMappingValues = false;
      WantsEventInAvoidZoneStateValues = false;
      WantsEventGPSAccuracyValues = false;
      WantsEventPositioningTechValues = false;
      WantsTempWarningLevelMinValues = false;
      WantsTempWarningLevelMaxValues = false;
      WantsTargetMDPValues = false;
      WantsLayerIDValues = false;
      WantsTargetCCAValues = false;
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
      WantsEventMinElevMappingValues = true;
      WantsEventInAvoidZoneStateValues = true;
      WantsEventGPSAccuracyValues = true;
      WantsEventPositioningTechValues = true;
      WantsTempWarningLevelMinValues = true;
      WantsTempWarningLevelMaxValues = true;
      WantsTargetMDPValues = true;
      WantsLayerIDValues = true;
      WantsTargetCCAValues = true;
    }

    /// <summary>
    /// Converts the set of event population flags into a bit-flagged integer
    /// </summary>
    /// <returns></returns>
    public Int32 GetFlags()
    {
      return ((WantsTargetCCVValues ? 1 : 0) * 0x1) |
             ((WantsTargetPassCountValues ? 1 : 0) * 0x2) |
             ((WantsTargetLiftThicknessValues ? 1 : 0) * 0x4) |
             ((WantsEventDesignNameValues ? 1 : 0) * 0x8) |
             ((WantsEventVibrationStateValues ? 1 : 0) * 0x10) |
             ((WantsEventAutoVibrationStateValues ? 1 : 0) * 0x20) |
             ((WantsEventICFlagsValues ? 1 : 0) * 0x40) |
             ((WantsEventMachineGearValues ? 1 : 0) * 0x80) |
             ((WantsEventMachineCompactionRMVJumpThreshold ? 1 : 0) * 0x100) |
             ((WantsEventMachineAutomaticsValues ? 1 : 0) * 0x200) |
             ((WantsEventMapResetValues ? 1 : 0) * 0x400) |
             ((WantsEventMinElevMappingValues ? 1 : 0) * 0x800) |
             ((WantsEventInAvoidZoneStateValues ? 1 : 0) * 0x1000) |
             ((WantsEventGPSAccuracyValues ? 1 : 0) * 0x2000) |
             ((WantsEventPositioningTechValues ? 1 : 0) * 0x4000) |
             ((WantsTempWarningLevelMinValues ? 1 : 0) * 0x8000) |
             ((WantsTempWarningLevelMaxValues ? 1 : 0) * 0x10000) |
             ((WantsTargetMDPValues ? 1 : 0) * 0x20000) |
             ((WantsLayerIDValues ? 1 : 0) * 0x40000) |
             ((WantsTargetCCAValues ? 1 : 0) * 0x80000);
    }

    /// <summary>
    /// Converts a bit-flagged integer into the set of event population flags
    /// </summary>
    /// <param name="flags"></param>
    public void SetFromFlags(UInt32 flags)
    {
      WantsTargetCCVValues = (flags & 0x1) != 0;
      WantsTargetPassCountValues = (flags & 0x2) != 0;
      WantsTargetLiftThicknessValues = (flags & 0x4) != 0;
      WantsEventDesignNameValues = (flags & 0x8) != 0;
      WantsEventVibrationStateValues = (flags & 0x10) != 0;
      WantsEventAutoVibrationStateValues = (flags & 0x20) != 0;
      WantsEventICFlagsValues = (flags & 0x40) != 0;
      WantsEventMachineGearValues = (flags & 0x80) != 0;
      WantsEventMachineCompactionRMVJumpThreshold = (flags & 0x100) != 0;
      WantsEventMachineAutomaticsValues = (flags & 0x200) != 0;
      WantsEventMapResetValues = (flags & 0x400) != 0;
      WantsEventMinElevMappingValues = (flags & 0x800) != 0;
      WantsEventInAvoidZoneStateValues = (flags & 0x1000) != 0;
      WantsEventGPSAccuracyValues = (flags & 0x2000) != 0;
      WantsEventPositioningTechValues = (flags & 0x4000) != 0;
      WantsTempWarningLevelMinValues = (flags & 0x08000) != 0;
      WantsTempWarningLevelMaxValues = (flags & 0x10000) != 0;
      WantsTargetMDPValues = (flags & 0x20000) != 0;
      WantsLayerIDValues = (flags & 0x40000) != 0;
      WantsTargetCCAValues = (flags & 0x80000) != 0;
    }

    /// <summary>
    /// Calculates the a set of lift build setting flags from the required data type and other lift build settings
    /// </summary>
    /// <param name="ProfileTypeRequired"></param>
    /// <param name="CompactionSummaryInLiftBuildSettings"></param>
    /// <param name="WorkInProgressSummaryInLiftBuildSettings"></param>
    /// <param name="ThicknessInProgressInLiftBuildSettings"></param>
    public static void CalculateFlags(GridDataType ProfileTypeRequired,
      //todo const LiftBuildSettings: TICLiftBuildSettings;
      out bool CompactionSummaryInLiftBuildSettings,
      out bool WorkInProgressSummaryInLiftBuildSettings,
      out bool ThicknessInProgressInLiftBuildSettings)
    {
      if (ProfileTypeRequired == GridDataType.All || ProfileTypeRequired == GridDataType.CCV ||
          ProfileTypeRequired == GridDataType.CCVPercent)
      {
        CompactionSummaryInLiftBuildSettings = false; // TODO = LiftBuildSettings.CCVSummaryTypes<>[];
        WorkInProgressSummaryInLiftBuildSettings = CompactionSummaryInLiftBuildSettings;
        ThicknessInProgressInLiftBuildSettings = CompactionSummaryInLiftBuildSettings;
      }
      else if (ProfileTypeRequired == GridDataType.All || ProfileTypeRequired == GridDataType.MDP ||
               ProfileTypeRequired == GridDataType.MDPPercent)
      {
        CompactionSummaryInLiftBuildSettings = false; // TODO = LiftBuildSettings.MDPSummaryTypes<>[];
        WorkInProgressSummaryInLiftBuildSettings = CompactionSummaryInLiftBuildSettings;
        ThicknessInProgressInLiftBuildSettings = CompactionSummaryInLiftBuildSettings;
      }

      if (ProfileTypeRequired == GridDataType.All || ProfileTypeRequired == GridDataType.CCA ||
          ProfileTypeRequired == GridDataType.CCAPercent)
      {
        CompactionSummaryInLiftBuildSettings = true;
        WorkInProgressSummaryInLiftBuildSettings = CompactionSummaryInLiftBuildSettings;
        ThicknessInProgressInLiftBuildSettings = CompactionSummaryInLiftBuildSettings;
      }
      else
      {
        CompactionSummaryInLiftBuildSettings = false;
        WorkInProgressSummaryInLiftBuildSettings = false;
        ThicknessInProgressInLiftBuildSettings = false;
      }
    }

    /// <summary>
    /// Prepares the set of event population control flags depending on the requested data type, filter, client grid
    /// and lift build related settings
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="passFilter"></param>
    /// <param name="clientGrid"></param>
    public void PreparePopulationControl(GridDataType profileTypeRequired,
      // todo const LiftBuildSettings: TICLiftBuildSettings;
      CellPassAttributeFilter passFilter,
      IClientLeafSubGrid clientGrid)
    {
      CalculateFlags(profileTypeRequired, // todo LiftBuildSettings,
        out bool CompactionSummaryInLiftBuildSettings, out bool WorkInProgressSummaryInLiftBuildSettings,
        out bool ThicknessInProgressInLiftBuildSettings);

      Clear();

      WantsTargetPassCountValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsTargetPassCountValues) != 0;
      WantsEventAutoVibrationStateValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventAutoVibrationStateValues) != 0;
      WantsEventICFlagsValues = (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventICFlagsValues) != 0;
      WantsEventMachineGearValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventMachineGearValues) != 0 ||
        passFilter.HasMachineDirectionFilter;
      WantsEventMachineCompactionRMVJumpThreshold = (clientGrid.EventPopulationFlags &
                                                     PopulationControlFlags
                                                       .WantsEventMachineCompactionRMVJumpThreshold) != 0;
      WantsTempWarningLevelMinValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsTempWarningLevelMinValues) != 0;
      WantsTempWarningLevelMaxValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsTempWarningLevelMaxValues) != 0;
      WantsEventMapResetValues =
        false; //todo LiftBuildSettings.LiftDetectionType in [icldtMapReset, icldtAutoMapReset];
      WantsEventDesignNameValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventDesignNameValues) != 0 ||
        passFilter.HasDesignFilter ||
        WantsEventMapResetValues;
      WantsTargetCCVValues = (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsTargetCCVValues) != 0 ||
                             CompactionSummaryInLiftBuildSettings ||
                             WorkInProgressSummaryInLiftBuildSettings;
      WantsTargetMDPValues = (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsTargetMDPValues) != 0 ||
                             CompactionSummaryInLiftBuildSettings ||
                             WorkInProgressSummaryInLiftBuildSettings;
      WantsTargetCCAValues = (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsTargetCCAValues) != 0 ||
                             CompactionSummaryInLiftBuildSettings ||
                             WorkInProgressSummaryInLiftBuildSettings;
      WantsTargetLiftThicknessValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsTargetThicknessValues) !=
        0 || ThicknessInProgressInLiftBuildSettings
          || WorkInProgressSummaryInLiftBuildSettings; // todo || (LiftBuildSettings.LiftDetectionType in [icldtAutomatic, icldtAutoMapReset]);
      WantsEventVibrationStateValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventVibrationStateValues) != 0 ||
        passFilter.HasVibeStateFilter ||
        (profileTypeRequired == GridDataType.All ||
         profileTypeRequired == GridDataType.CCV ||
         profileTypeRequired == GridDataType.CCVPercent ||
         profileTypeRequired == GridDataType.RMV ||
         profileTypeRequired == GridDataType.Frequency ||
         profileTypeRequired == GridDataType.Amplitude);
      WantsEventMinElevMappingValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventMinElevMappingValues) != 0 ||
        passFilter.HasMinElevMappingFilter;
      WantsEventInAvoidZoneStateValues =
        false; // todo (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsInAvoidZoneStateValues) != 0 || passFilter.HasAvoidZoneStateFilter;
      WantsEventGPSAccuracyValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventGPSAccuracyValues) != 0 ||
        passFilter.HasGPSAccuracyFilter || passFilter.HasGPSToleranceFilter;
      WantsEventPositioningTechValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventPositioningTechValues) != 0 ||
        passFilter.HasPositioningTechFilter;
      WantsEventMachineAutomaticsValues =
        (clientGrid.EventPopulationFlags & PopulationControlFlags.WantsEventMachineAutomaticsValues) != 0 ||
        passFilter.HasGCSGuidanceModeFilter;
      WantsLayerIDValues =
        false; // Todo (LiftBuildSettings.LiftDetectionType = icldtTagfile) || passFilter.HasLayerIDFilter || (LiftBuildSettings.LiftDetectionType in[icldtMapReset, icldtAutoMapReset]);
    }

    /// <summary>
    /// Prepares the set of event population control flags depending on the requested data type and filter
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="passFilter"></param>
    public void PreparePopulationControl(GridDataType profileTypeRequired,
      // todo const LiftBuildSettings: TICLiftBuildSettings;
      CellPassAttributeFilter passFilter)
    {
      CalculateFlags(profileTypeRequired, // todo LiftBuildSettings,
        out bool CompactionSummaryInLiftBuildSettings, out bool WorkInProgressSummaryInLiftBuildSettings,
        out bool ThicknessInProgressInLiftBuildSettings);

      Clear();

      WantsTargetPassCountValues = profileTypeRequired == GridDataType.All;

      WantsTempWarningLevelMinValues = profileTypeRequired == GridDataType.All;
      WantsTempWarningLevelMaxValues = profileTypeRequired == GridDataType.All;

      WantsEventMachineGearValues = passFilter.HasMachineDirectionFilter;
      WantsEventMapResetValues =
        false; //todo LiftBuildSettings.LiftDetectionType in [icldtMapReset, icldtAutoMapReset];
      WantsEventDesignNameValues = passFilter.HasDesignFilter || WantsEventMapResetValues;

      WantsTargetCCVValues = CompactionSummaryInLiftBuildSettings || WorkInProgressSummaryInLiftBuildSettings;
      WantsTargetMDPValues = CompactionSummaryInLiftBuildSettings || WorkInProgressSummaryInLiftBuildSettings;
      WantsTargetCCAValues = CompactionSummaryInLiftBuildSettings || WorkInProgressSummaryInLiftBuildSettings;

      WantsTargetLiftThicknessValues =
        false; // todo ThicknessInProgressInLiftBuildSettings || WorkInProgressSummaryInLiftBuildSettings ||
      // todo (LiftBuildSettings.LiftDetectionType in [icldtAutomatic, icldtAutoMapReset]);

      WantsEventVibrationStateValues = passFilter.HasVibeStateFilter ||
                                       (profileTypeRequired == GridDataType.All ||
                                        profileTypeRequired == GridDataType.CCV ||
                                        profileTypeRequired == GridDataType.CCVPercent ||
                                        profileTypeRequired == GridDataType.RMV ||
                                        profileTypeRequired == GridDataType.Frequency ||
                                        profileTypeRequired == GridDataType.Amplitude);

      WantsEventMinElevMappingValues = passFilter.HasMinElevMappingFilter;
      WantsEventInAvoidZoneStateValues = false; // todo passFilter.HasAvoidZoneStateFilter;
      WantsEventGPSAccuracyValues = passFilter.HasGPSAccuracyFilter || passFilter.HasGPSToleranceFilter;
      WantsEventPositioningTechValues = passFilter.HasPositioningTechFilter;
      WantsEventMachineAutomaticsValues = passFilter.HasGCSGuidanceModeFilter;
      WantsLayerIDValues =
        false; // todo (LiftBuildSettings.LiftDetectionType = icldtTagfile) || passFilter.HasLayerIDFilter || (LiftBuildSettings.LiftDetectionType in [icldtMapReset, icldtAutoMapReset]);
    }
  }
}
