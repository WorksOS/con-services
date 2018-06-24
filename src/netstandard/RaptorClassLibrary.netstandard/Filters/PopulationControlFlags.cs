using System;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// A set of bit flag enum values to encode a set of 'wants' subgrids have with respect to machine events
  /// </summary>
  [Flags]
  public enum PopulationControlFlags
  {
    None = 0x0,
    WantsTargetCCVValues = 0x1,
    WantsTargetPassCountValues = 0x2,
    WantsTargetThicknessValues = 0x4,
    WantsEventDesignNameValues = 0x8,
    WantsEventVibrationStateValues = 0x10,
    WantsEventAutoVibrationStateValues = 0x20,
    WantsEventICFlagsValues = 0x40,
    WantsEventMachineGearValues = 0x80,
    WantsEventMachineCompactionRMVJumpThreshold = 0x100,
    WantsEventMachineAutomaticsValues = 0x200,
    WantsEventMapResetValues = 0x400,
    WantsEventMinElevMappingValues = 0x800,
    WantsEventInAvoidZoneStateValues = 0x1000,
    WantsEventGPSAccuracyValues = 0x2000,
    WantsEventPositioningTechValues = 0x4000,
    WantsTempWarningLevelMinValues = 0x08000,
    WantsTempWarningLevelMaxValues = 0x10000,
    WantsTargetMDPValues = 0x20000,
    WantsLayerIDValues = 0x40000,
    WantsTargetCCAValues = 0x080000
  }
}
