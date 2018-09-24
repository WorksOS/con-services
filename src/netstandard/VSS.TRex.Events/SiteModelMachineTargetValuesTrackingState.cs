using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
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
    public IProductionEventLists MachineTargetValues; 

    public bool TrackingUseMachineRMVThreshold;
    public short TrackingOverrideRMVJumpThreshold;

    public short TargetMDP;
    public SiteModelMachineTargetValueTrackingState<short> TargetMDP_Tracking;

    public byte TargetCCA;
    public SiteModelMachineTargetValueTrackingState<byte> TargetCCA_Tracking;

    public short TargetCCV;
    public SiteModelMachineTargetValueTrackingState<short> TargetCCV_Tracking;

    public ushort TargetPassCount;
    public SiteModelMachineTargetValueTrackingState<ushort> TargetPassCount_Tracking;

    public float TargetLiftThickness;
    public SiteModelMachineTargetValueTrackingState<float> TargetLiftThickness_Tracking;

    public ushort TempWarningLevelMin;
    public SiteModelMachineTargetValueTrackingState<ushort> TempWarningLevelMin_Tracking;

    public ushort TempWarningLevelMax;
    public SiteModelMachineTargetValueTrackingState<ushort> TempWarningLevelMax_Tracking;

    public int EventDesignNameID;
    public SiteModelMachineTargetValueTrackingState<int> EventDesignNameID_Tracking;

    public VibrationState EventVibrationState;
    public SiteModelMachineTargetValueTrackingState<VibrationState> EventVibrationState_Tracking;

    public AutoVibrationState EventAutoVibrationState;
    public SiteModelMachineTargetValueTrackingState<AutoVibrationState> EventAutoVibrationState_Tracking;

    public bool MinElevMappingState;
    public SiteModelMachineTargetValueTrackingState<bool> MinElevMappingState_Tracking;

    public GPSAccuracyAndTolerance GPSAccuracyAndTolerance;
    public SiteModelMachineTargetValueTrackingState<GPSAccuracyAndTolerance> GPSAccuracyState_Tracking;

    public PositioningTech PositioningTechState;
    public SiteModelMachineTargetValueTrackingState<PositioningTech> PositioningTechState_Tracking;

    public byte EventICFlag;
    public SiteModelMachineTargetValueTrackingState<byte> EventICFlag_Tracking;
    
    public MachineGear EventMachineGear;
    public SiteModelMachineTargetValueTrackingState<MachineGear> EventMachineGear_Tracking;

    public short EventMachineRMVThreshold;
    public SiteModelMachineTargetValueTrackingState<short> EventMachineRMVThreshold_Tracking;

    public MachineAutomaticsMode EventMachineAutomatics;
    public SiteModelMachineTargetValueTrackingState<MachineAutomaticsMode> EventMachineAutomatics_Tracking;

    public ushort EventLayerID;
    public SiteModelMachineTargetValueTrackingState<ushort> EventLayerID_Tracking;

    // Todo - map resets not included
//    public DateTime EventMapResetPriorDate;
    //    EventMapResetDesignID        : TICDesignNameID;
    //    EventMapReset_Tracking       : TTICSiteModelMachineTargetValueTrackingState;

    public SiteModelMachineTargetValuesTrackingState()
    {
    }

    public void Initialise(IFilteredValuePopulationControl populationControl)
    {
      TrackingUseMachineRMVThreshold = false;
      TrackingOverrideRMVJumpThreshold = CellPassConsts.NullRMV;

      if (populationControl.WantsTargetCCVValues)
      {
        TargetCCV = CellPassConsts.NullCCV;
        TargetCCV_Tracking = new SiteModelMachineTargetValueTrackingState<short>(MachineTargetValues, ProductionEventType.TargetCCV);
      }

      if (populationControl.WantsTargetMDPValues)
      {
        TargetMDP = CellPassConsts.NullMDP;
        TargetMDP_Tracking = new SiteModelMachineTargetValueTrackingState<short>(MachineTargetValues, ProductionEventType.TargetMDP);
      }

      if (populationControl.WantsTargetCCAValues)
      {
        TargetCCA = CellPassConsts.NullCCA;
        TargetCCA_Tracking = new SiteModelMachineTargetValueTrackingState<byte>(MachineTargetValues, ProductionEventType.TargetCCA);
      }

      if (populationControl.WantsTargetPassCountValues)
      {
        TargetPassCount = 0; // kICNullPassCountValue;
        TargetPassCount_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>(MachineTargetValues, ProductionEventType.TargetPassCount);
      }

      if (populationControl.WantsTargetLiftThicknessValues)
      {
        TargetLiftThickness = CellPassConsts.NullHeight;
        TargetLiftThickness_Tracking = new SiteModelMachineTargetValueTrackingState<float>(MachineTargetValues, ProductionEventType.TargetLiftThickness);
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
        EventDesignNameID_Tracking = new SiteModelMachineTargetValueTrackingState<int>(MachineTargetValues, ProductionEventType.DesignChange);
      }

      if (populationControl.WantsEventVibrationStateValues)
      {
        EventVibrationState = VibrationState.Invalid;
        EventVibrationState_Tracking = new SiteModelMachineTargetValueTrackingState<VibrationState>(MachineTargetValues, ProductionEventType.VibrationStateChange);
      }

      if (populationControl.WantsEventAutoVibrationStateValues)
      {
        EventAutoVibrationState = AutoVibrationState.Unknown;
        EventAutoVibrationState_Tracking = new SiteModelMachineTargetValueTrackingState<AutoVibrationState>(MachineTargetValues, ProductionEventType.AutoVibrationStateChange);
      }

      if (populationControl.WantsEventMinElevMappingValues)
      {
        MinElevMappingState = false;
        MinElevMappingState_Tracking = new SiteModelMachineTargetValueTrackingState<bool>(MachineTargetValues, ProductionEventType.MinElevMappingStateChange);
      }

      if (populationControl.WantsEventICFlagsValues)
      {
        EventICFlag = 0;
        EventICFlag_Tracking = new SiteModelMachineTargetValueTrackingState<byte>(MachineTargetValues, ProductionEventType.ICFlagsChange);
      }

      if (populationControl.WantsEventMachineGearValues)
      {
        EventMachineGear = MachineGear.Null;
        EventMachineGear_Tracking = new SiteModelMachineTargetValueTrackingState<MachineGear>(MachineTargetValues, ProductionEventType.MachineGearChange);
      }

      if (populationControl.WantsEventMachineCompactionRMVJumpThreshold)
      {
        EventMachineRMVThreshold = CellPassConsts.NullRMV;
        EventMachineRMVThreshold_Tracking = new SiteModelMachineTargetValueTrackingState<short>(MachineTargetValues, ProductionEventType.MachineRMVJumpValueChange);
      }

      if (populationControl.WantsEventMachineAutomaticsValues)
      {
        EventMachineAutomatics = MachineAutomaticsMode.Unknown;
        EventMachineAutomatics_Tracking = new SiteModelMachineTargetValueTrackingState<MachineAutomaticsMode>(MachineTargetValues, ProductionEventType.MachineAutomaticsChange);
      }

      if (populationControl.WantsEventGPSAccuracyValues)
      {
        GPSAccuracyAndTolerance = new GPSAccuracyAndTolerance(GPSAccuracy.Unknown, CellPassConsts.NullGPSTolerance);
        GPSAccuracyState_Tracking = new SiteModelMachineTargetValueTrackingState<GPSAccuracyAndTolerance>(MachineTargetValues, ProductionEventType.GPSAccuracyChange);
      }

      if (populationControl.WantsEventPositioningTechValues)
      {
        PositioningTechState = PositioningTech.Unknown;
        PositioningTechState_Tracking = new SiteModelMachineTargetValueTrackingState<PositioningTech>(MachineTargetValues, ProductionEventType.PositioningTech);
      }

      if (populationControl.WantsTempWarningLevelMinValues)
      {
        TempWarningLevelMin = CellPassConsts.NullMaterialTemperatureValue;
        TempWarningLevelMin_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>(MachineTargetValues, ProductionEventType.TempWarningLevelMinChange);
      }

      if (populationControl.WantsTempWarningLevelMaxValues)
      {
        TempWarningLevelMax = CellPassConsts.NullMaterialTemperatureValue;
        TempWarningLevelMax_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>(MachineTargetValues, ProductionEventType.TempWarningLevelMaxChange);
      }

      if (populationControl.WantsLayerIDValues)
      {
        EventLayerID = CellPassConsts.NullLayerID;
        EventLayerID_Tracking = new SiteModelMachineTargetValueTrackingState<ushort>(MachineTargetValues, ProductionEventType.LayerID);
      }
    }
  }
}
