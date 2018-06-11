using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events;
using VSS.TRex.Types;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Sets target and event values into a filtered cell pass based on the machine target values and filtered value population control
  /// </summary>
  public static class FiltersValuePopulation
  {
    private static ILogger Log = Logging.Logger.CreateLogger("FiltersValuePopulation");

    public static void PopulateFilteredValues(ProductionEventLists values,
      FilteredValuePopulationControl PopulationControl,
      ref FilteredPassData filteredPass)
    {
      DateTime _Time;
      // todo _EventInAvoidZone :TICInAvoidZoneState;
      // todo bool UseMachineRMVThreshold;
      // todo short OverrideRMVJumpThreshold;

      if (values == null)
      {
        Log.LogDebug(
          $"***Error*** MachineTargetValues supplied to PopulateFilteredValues is null. PopulationControl = {PopulationControl.GetFlags():X}");
        return;
      }

      _Time = filteredPass.FilteredPass.Time;

      if (PopulationControl.WantsTargetCCVValues)
        filteredPass.TargetValues.TargetCCV = values.TargetCCVStateEvents.GetValueAtDate(_Time, out int _);

      if (PopulationControl.WantsTargetMDPValues)
        filteredPass.TargetValues.TargetMDP = values.TargetMDPStateEvents.GetValueAtDate(_Time, out int _);

      if (PopulationControl.WantsTargetCCAValues)
        filteredPass.TargetValues.TargetCCA = values.TargetCCAStateEvents.GetValueAtDate(_Time, out int _);

      if (PopulationControl.WantsTargetPassCountValues)
        filteredPass.TargetValues.TargetPassCount = values.TargetPassCountStateEvents.GetValueAtDate(_Time, out int _);

      if (PopulationControl.WantsTargetLiftThicknessValues)
        filteredPass.TargetValues.TargetLiftThickness = values.TargetLiftThicknessStateEvents.GetValueAtDate(_Time, out int _);

      // Design Name...
      if (PopulationControl.WantsEventDesignNameValues)
        filteredPass.EventValues.EventDesignNameID = values.DesignNameIDStateEvents.GetValueAtDate(_Time, out int _);

      // Vibration State...
      if (PopulationControl.WantsEventVibrationStateValues)
      {
        filteredPass.EventValues.EventVibrationState = values.VibrationStateEvents.GetValueAtDate(_Time, out int _);

        if (filteredPass.EventValues.EventVibrationState != VibrationState.On)
          filteredPass.FilteredPass.SetFieldsForVibeStateOff();
      }

      // Auto Vibration State...
      if (PopulationControl.WantsEventAutoVibrationStateValues)
        filteredPass.EventValues.EventAutoVibrationState =
          values.AutoVibrationStateEvents.GetValueAtDate(_Time, out int _);

      // IC Flags...
      if (PopulationControl.WantsEventICFlagsValues)
        filteredPass.EventValues.EventFlags = values.ICFlagsStateEvents.GetValueAtDate(_Time, out int _);

      // Machine Gear...
      if (PopulationControl.WantsEventMachineGearValues)
        filteredPass.EventValues.EventMachineGear = values.MachineGearStateEvents.GetValueAtDate(_Time, out int _);

      // RMV Jump Threshhold...
      if (PopulationControl.WantsEventMachineCompactionRMVJumpThreshold)
      {
        throw new NotImplementedException("PopulationControl.WantsEventMachineCompactionRMVJumpThreshold not implemented");
        /*
        if TICSiteModel(Owner).GetMachineRMVOverrideState(filteredPass.FilteredPass.InternalSiteModelMachineIndex,
          UseMachineRMVThreshold, OverrideRMVJumpThreshold))
        {
          if (UseMachineRMVThreshold)
            filteredPass.EventValues.EventMachineRMVThreshold =
              values.RMVJumpThresholdEvents.GetValueAtDate(_Time, out int _);
          else
            filteredPass.EventValues.EventMachineRMVThreshold = OverrideRMVJumpThreshold;
        }
        */
      }

      // Machine Automatic States...
      if (PopulationControl.WantsEventMachineAutomaticsValues)
        filteredPass.EventValues.EventMachineAutomatics =
          values.MachineAutomaticsStateEvents.GetValueAtDate(_Time, out int _);

      if (PopulationControl.WantsEventMapResetValues)
      {
        throw new NotImplementedException("PopulationControl.WantsEventMapResetValues not implemented");
        /* 
        filteredPass.EventValues.MapReset_DesignNameID = values.?????.GetValueAtDate(_Time, out int _);
        // LocateClosestPreviousMapResetAtDate(MachineTargetValues, _Time, MapReset_PriorDate, MapReset_DesignNameID);
        */
      }

      if (PopulationControl.WantsEventMinElevMappingValues)
        filteredPass.EventValues.EventMinElevMapping =
          values.MinElevMappingStateEvents.GetValueAtDate(_Time, out int _);

      if (PopulationControl.WantsEventInAvoidZoneStateValues)
      {
        throw new NotImplementedException("PopulationControl.WantsEventInAvoidZoneStateValues not implemented");
        /*
        LocateInAvoidZone2DStateValueAtDate(MachineTargetValues, _Time, _EventInAvoidZone);
        EventValues.EventInAvoidZoneState = _EventInAvoidZone;

        LocateInAvoidZoneUSStateValueAtDate(MachineTargetValues, _Time, _EventInAvoidZone);
        EventValues.EventInAvoidZoneState = EventValues.EventInAvoidZoneState || _EventInAvoidZone;
        */
      }

      if (PopulationControl.WantsEventGPSAccuracyValues)
      {
        GPSAccuracyAndTolerance value = values.GPSAccuracyAndToleranceStateEvents.GetValueAtDate(_Time, out int _);

        filteredPass.EventValues.GPSAccuracy = value.GPSAccuracy;
        filteredPass.EventValues.GPSTolerance = value.GPSTolerance;
      }

      if (PopulationControl.WantsEventPositioningTechValues)
          filteredPass.EventValues.PositioningTechnology = values.PositioningTechStateEvents.GetValueAtDate(_Time, out int _);

      if (PopulationControl.WantsTempWarningLevelMinValues)
      {
        filteredPass.TargetValues.TempWarningLevelMin =values.TargetMinMaterialTemperature.GetValueAtDate(_Time, out int _);
        filteredPass.TargetValues.TempWarningLevelMax = values.TargetMaxMaterialTemperature.GetValueAtDate(_Time, out int _);
      }

      if (PopulationControl.WantsLayerIDValues)
        filteredPass.EventValues.LayerID = values.LayerIDStateEvents.GetValueAtDate(_Time, out int _);
    }
  }
}
