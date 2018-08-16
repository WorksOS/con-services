using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters;
using VSS.TRex.Types;

namespace VSS.TRex.Events
{
  /// <summary>
  /// Contains target and event tracking for machines within a sitemodel during cell profile analysis operations
  /// </summary>
  public class SiteModelMachineTargetValuesTrackingState
  {
    /// <summary>
    /// A reference to the machine production event lists for the machines within the sitemodel
    /// </summary>
    public ProductionEventLists MachineTargetValues; 

    public bool TrackingUseMachineRMVThreshold;
    public short TrackingOverrideRMVJumpThreshold;

    public short TargetMDP;
    public SiteModelMachineTargetValueTrackingState<short> TargetMDP_Tracking = new SiteModelMachineTargetValueTrackingState<short>();

    public byte TargetCCA;
    public SiteModelMachineTargetValueTrackingState<byte> TargetCCA_Tracking = new SiteModelMachineTargetValueTrackingState<byte>();

    public short TargetCCV;
    public SiteModelMachineTargetValueTrackingState<short> TargetCCV_Tracking = new SiteModelMachineTargetValueTrackingState<short>();

    public ushort TargetPassCount;
    public SiteModelMachineTargetValueTrackingState<ushort> TargetPassCount_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>();

    public float TargetLiftThickness;
    public SiteModelMachineTargetValueTrackingState<float> TargetLiftThickness_Tracking = new SiteModelMachineTargetValueTrackingState<float>();

    public ushort TempWarningLevelMin;
    public SiteModelMachineTargetValueTrackingState<ushort> TempWarningLevelMin_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>();

    public ushort TempWarningLevelMax;
    public SiteModelMachineTargetValueTrackingState<ushort> TempWarningLevelMax_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>();

    public int EventDesignNameID;
    public SiteModelMachineTargetValueTrackingState<int> EventDesignNameID_Tracking = new SiteModelMachineTargetValueTrackingState<int>();

    public VibrationState EventVibrationState;
    public SiteModelMachineTargetValueTrackingState<VibrationState> EventVibrationState_Tracking = new SiteModelMachineTargetValueTrackingState<VibrationState>();

    public AutoVibrationState EventAutoVibrationState;
    public SiteModelMachineTargetValueTrackingState<AutoVibrationState> EventAutoVibrationState_Tracking = new SiteModelMachineTargetValueTrackingState<AutoVibrationState>();

    public bool MinElevMappingState;
    public SiteModelMachineTargetValueTrackingState<bool> MinElevMappingState_Tracking = new SiteModelMachineTargetValueTrackingState<bool>();

    public GPSAccuracyAndTolerance GPSAccuracyAndTolerance;
    public SiteModelMachineTargetValueTrackingState<GPSAccuracyAndTolerance> GPSAccuracyState_Tracking = new SiteModelMachineTargetValueTrackingState<GPSAccuracyAndTolerance>();

    public PositioningTech PositioningTechState;
    public SiteModelMachineTargetValueTrackingState<PositioningTech> PositioningTechState_Tracking = new SiteModelMachineTargetValueTrackingState<PositioningTech>();

    public byte EventICFlag;
    public SiteModelMachineTargetValueTrackingState<byte> EventICFlag_Tracking = new SiteModelMachineTargetValueTrackingState<byte>();
    
    public MachineGear EventMachineGear;
    public SiteModelMachineTargetValueTrackingState<MachineGear> EventMachineGear_Tracking = new SiteModelMachineTargetValueTrackingState<MachineGear>();

    public short EventMachineRMVThreshold;
    public SiteModelMachineTargetValueTrackingState<short> EventMachineRMVThreshold_Tracking = new SiteModelMachineTargetValueTrackingState<short>();

    public MachineAutomaticsMode EventMachineAutomatics;
    public SiteModelMachineTargetValueTrackingState<MachineAutomaticsMode> EventMachineAutomatics_Tracking = new SiteModelMachineTargetValueTrackingState<MachineAutomaticsMode>();

    public ushort EventLayerID;
    public SiteModelMachineTargetValueTrackingState<ushort> EventLayerID_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>();

    // Todo - avoidance zones not included
    //    InAvoidZone2DState          : TICInAvoidZoneState;
    //    InAvoidZone2DState_Tracking : TTICSiteModelMachineTargetValueTrackingState;

    //    InAvoidZoneUSState          : TICInAvoidZoneState;
    //    InAvoidZoneUSState_Tracking : TTICSiteModelMachineTargetValueTrackingState;

    // Todo - map resets not included
//    public DateTime EventMapResetPriorDate;
    //    EventMapResetDesignID        : TICDesignNameID;
    //    EventMapReset_Tracking       : TTICSiteModelMachineTargetValueTrackingState;

    public void Initialise(FilteredValuePopulationControl populationControl)
    {
      TrackingUseMachineRMVThreshold = false;
      TrackingOverrideRMVJumpThreshold = CellPassConsts.NullRMV;

      if (populationControl.WantsTargetCCVValues)
      {
        TargetCCV = CellPassConsts.NullCCV;
        TargetCCV_Tracking.Initialise();
      }

      if (populationControl.WantsTargetMDPValues)
      {
        TargetMDP = CellPassConsts.NullMDP;
        TargetMDP_Tracking.Initialise();
      }

      if (populationControl.WantsTargetCCAValues)
      {
        TargetCCA = CellPassConsts.NullCCA;
        TargetCCA_Tracking.Initialise();
      }

      if (populationControl.WantsTargetPassCountValues)
      {
        TargetPassCount = 0; // kICNullPassCountValue;
        TargetPassCount_Tracking.Initialise();
      }

      if (populationControl.WantsTargetLiftThicknessValues)
      {
        TargetLiftThickness = CellPassConsts.NullHeight;
        TargetLiftThickness_Tracking.Initialise();
      }

      /* TODO map reset events not included
      if (populationControl.WantsEventMapResetValues)
      {
        EventMapResetPriorDate = 0;
        EventMapResetDesignID = kNoDesignNameID;
        EventMapReset_Tracking.Initialise();
      }
      */

      if (populationControl.WantsEventDesignNameValues)
      {
        EventDesignNameID = Consts.kNoDesignNameID;
        EventDesignNameID_Tracking.Initialise();
      }

      if (populationControl.WantsEventVibrationStateValues)
      {
        EventVibrationState = VibrationState.Invalid;
        EventVibrationState_Tracking.Initialise();
      }

      if (populationControl.WantsEventAutoVibrationStateValues)
      {
        EventAutoVibrationState = AutoVibrationState.Unknown;
        EventAutoVibrationState_Tracking.Initialise();
      }

      if (populationControl.WantsEventMinElevMappingValues)
      {
        MinElevMappingState = false;
        MinElevMappingState_Tracking.Initialise();
      }

      /* todo... Avoidance zones not included
      if (populationControl.WantsEventInAvoidZoneStateValues)
      {
        InAvoidZone2DState = 0;
        InAvoidZone2DState_Tracking.Initialise();

        InAvoidZoneUSState = 0;
        InAvoidZoneUSState_Tracking.Initialise();
      }
      */

      if (populationControl.WantsEventICFlagsValues)
      {
        EventICFlag = 0;
        EventICFlag_Tracking.Initialise();
      }

      if (populationControl.WantsEventMachineGearValues)
      {
        EventMachineGear = MachineGear.Null;
        EventMachineGear_Tracking.Initialise();
      }

      if (populationControl.WantsEventMachineCompactionRMVJumpThreshold)
      {
        EventMachineRMVThreshold = CellPassConsts.NullRMV;
        EventMachineRMVThreshold_Tracking.Initialise();
      }

      if (populationControl.WantsEventMachineAutomaticsValues)
      {
        EventMachineAutomatics = MachineAutomaticsMode.Unknown;
        EventMachineAutomatics_Tracking.Initialise();
      }

      if (populationControl.WantsEventGPSAccuracyValues)
      {
        GPSAccuracyAndTolerance = new GPSAccuracyAndTolerance(GPSAccuracy.Unknown, CellPassConsts.NullGPSTolerance);
        GPSAccuracyState_Tracking.Initialise();
      }

      if (populationControl.WantsEventPositioningTechValues)
      {
        PositioningTechState = PositioningTech.Unknown;
        PositioningTechState_Tracking.Initialise();
      }

      if (populationControl.WantsTempWarningLevelMinValues)
      {
        TempWarningLevelMin = CellPassConsts.NullMaterialTemperatureValue;
        TempWarningLevelMin_Tracking.Initialise();
      }

      if (populationControl.WantsTempWarningLevelMaxValues)
      {
        TempWarningLevelMax = CellPassConsts.NullMaterialTemperatureValue;
        TempWarningLevelMax_Tracking.Initialise();
      }

      if (populationControl.WantsLayerIDValues)
      {
        EventLayerID = CellPassConsts.NullLayerID;
        EventLayerID_Tracking.Initialise();
      }
    }
  }
}
