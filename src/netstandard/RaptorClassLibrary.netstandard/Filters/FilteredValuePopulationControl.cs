using System;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Tracks the set of machine evnt sources that a query requires to satisfy its function
  /// </summary>
  public class FilteredValuePopulationControl
  {
    public bool WantsTargetCCVValues { get; set; }
    public bool WantsTargetPassCountValues { get; set; }
    public bool WantsTargetThicknessValues { get; set; }
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
    public bool WantsEventOnGroundValues { get; set; }
    public bool WantsTempWarningLevelMinValues { get; set; }
    public bool WantsTempWarningLevelMaxValues { get; set; }
    public bool WantsTargetMDPValues { get; set; }
    public bool WantsLayerIDValues { get; set; }
    public bool WantsTargetCCAValues { get; set; }

//var
//  kEmptyPopulationControl_BuildLiftsForCell : TFilteredValuePopulationControl;

    public bool AnySet()
    {
      return WantsTargetCCVValues ||
             WantsTargetPassCountValues ||
             WantsTargetThicknessValues ||
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
             WantsEventOnGroundValues ||
             WantsTempWarningLevelMinValues ||
             WantsTempWarningLevelMaxValues ||
             WantsTargetMDPValues ||
             WantsLayerIDValues ||
             WantsTargetCCAValues;
    }

    public void Clear()
    {
      WantsTargetCCVValues = false;
      WantsTargetPassCountValues = false;
      WantsTargetThicknessValues = false;
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
      WantsEventOnGroundValues = false;
      WantsTempWarningLevelMinValues = false;
      WantsTempWarningLevelMaxValues = false;
      WantsTargetMDPValues = false;
      WantsLayerIDValues = false;
      WantsTargetCCAValues = false;
    }

    public void Fill()
    {
      WantsTargetCCVValues = true;
      WantsTargetPassCountValues = true;
      WantsTargetThicknessValues = true;
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
      WantsEventOnGroundValues = true;
      WantsTempWarningLevelMinValues = true;
      WantsTempWarningLevelMaxValues = true;
      WantsTargetMDPValues = true;
      WantsLayerIDValues = true;
      WantsTargetCCAValues = true;
    }

    public Int32 GetFlags()
    {
      return ((WantsTargetCCVValues ? 1 : 0) * 0x1) |
             ((WantsTargetPassCountValues ? 1 : 0) * 0x2) |
             ((WantsTargetThicknessValues ? 1 : 0) * 0x4) |
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
             ((WantsEventOnGroundValues ? 1 : 0) * 0x8000) |
             ((WantsTempWarningLevelMinValues ? 1 : 0) * 0x10000) |
             ((WantsTempWarningLevelMaxValues ? 1 : 0) * 0x20000) |
             ((WantsTargetMDPValues ? 1 : 0) * 0x40000) |
             ((WantsLayerIDValues ? 1 : 0) * 0x80000) |
             ((WantsTargetCCAValues ? 1 : 0) * 0x100000);
    }

    public void SetFromFlags(UInt32 flags)
    {
      WantsTargetCCVValues = (flags & 0x1) != 0;
      WantsTargetPassCountValues = (flags & 0x2) != 0;
      WantsTargetThicknessValues = (flags & 0x4) != 0;
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
      WantsEventOnGroundValues = (flags & 0x8000) != 0;
      WantsTempWarningLevelMinValues = (flags & 0x10000) != 0;
      WantsTempWarningLevelMaxValues = (flags & 0x20000) != 0;
      WantsTargetMDPValues = (flags & 0x40000) != 0;
      WantsLayerIDValues = (flags & 0x80000) != 0;
      WantsTargetCCAValues = (flags & 0x100000) != 0;
    }

//Initialization
//  kEmptyPopulationControl_BuildLiftsForCell.Clear;

  }
}
