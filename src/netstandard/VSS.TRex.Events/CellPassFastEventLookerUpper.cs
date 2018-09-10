using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Filters.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Events
{
  /*
    This unit implements a very fast event lookup process designed to allow lists of cell passes
    to have a set of required event information retrieved from the machine events lists
    very quickly.

    The basic premise is that the list of cell passes are always processed in a strict time increasing
    order. This means that full event lookups are not necessary as it is often sufficient
    to determine if the time being used for the lookup is in the event time range of the
    event retrieved for the previous cell pass in the list.

    Compared with a date comparison, a full event lookup is an expensive operation.

    A complicating factor is that multiple machines may have contributed to the
    cell passes being processed. To handle this a tracking state containing an
    array of sets of state information, where each set of information tracks the most recent
    values looked up for particular events for that machine.

    In this way, a single linear pass of the cell passes is sufficient to perform all
    required loopkups in a way that minimises the number of required full event lookups.

    Each time the engine is invoked it increments a stamp which is used to indicate if particular
    values in the state relate to the current pass of cell event lookups. This is used to remove
    the need for expensive state clearance and setup in situations where there are many
    machines, yet few of the machines contribute to the cell passes.

    If we have not yet retrieved the first event value (TargetCCV_Tracking.Stamp <> Stamp) or if
    the last event value retrieved is out of date (_Time > TargetCCV_Tracking.EndDate) then the
    full event lookup is performed and cached into the appropriate tracking state for that
    value. Otherwise, the most recently looked up value is retrieved from the tracking
    state and assigned to the appropriate target or event value (TargetValues.TargetCCV = TargetCCV;)
  */

  public class CellPassFastEventLookerUpper : ICellPassFastEventLookerUpper
  {
    private static ILogger Log = Logging.Logger.CreateLogger<CellPassFastEventLookerUpper>();

    private int Stamp;

    private SiteModelMachineTargetValuesTrackingState[] MachinesValuesTrackingState;

    private ISiteModel _SiteModel;

    public ISiteModel SiteModel { get => _SiteModel;
      set
      {
        _SiteModel = value;
        MachinesValuesTrackingState = new SiteModelMachineTargetValuesTrackingState[_SiteModel?.Machines.Count ?? 0];
      }
    }

    private short LastMachineID;
    public SiteModelMachineTargetValuesTrackingState TrackingState;

    protected void IncrementStamp() => Stamp++;

    public CellPassFastEventLookerUpper(ISiteModel siteModel)
    {
      SiteModel = siteModel;
      Stamp = 0;

      ClearLastValues();
    }

    /// <summary>
    /// Initialise tracking state values to null
    /// </summary>
    public void ClearLastValues()
    {
      LastMachineID = -2;
      TrackingState = null;
    }

    public void PopulateFilteredValues(FilteredPassData[] passes,
      int firstPassIndex, int lastPassIndex,
      IFilteredValuePopulationControl populationControl,
      bool ignoreBussinessRulesRules)
    {
      if (firstPassIndex == -1 || lastPassIndex == -1)
        return; // Nothing to do

      IncrementStamp();

      bool ProcessInForwardDirection = lastPassIndex >= firstPassIndex;
      int TerminationIndex = ProcessInForwardDirection ? lastPassIndex + 1 : lastPassIndex - 1;

      int I = firstPassIndex;

      do
      {
        // SIGLogMessage.PublishNoODS(Nil, Format('Examining pass #%d, flags = $%x', [I, PopulationControl.GetFlags]), slmcDebug);

        // ********************************************************************************
        // *** Determine machine, machine targets and tracking state for this cell pass ***
        // ********************************************************************************

        short _MachineID = passes[I].FilteredPass.InternalSiteModelMachineIndex;
        DateTime _Time = passes[I].FilteredPass.Time;

        if (_MachineID != LastMachineID)
        {
          LastMachineID = _MachineID;
          TrackingState = MachinesValuesTrackingState[_MachineID];

          if (TrackingState == null)
          {
            TrackingState = new SiteModelMachineTargetValuesTrackingState();
            MachinesValuesTrackingState[_MachineID] = TrackingState;

            TrackingState.Initialise(populationControl);

            TrackingState.MachineTargetValues = SiteModel.MachinesTargetValues[_MachineID];

            if (TrackingState.MachineTargetValues == null)
            {
              Log.LogWarning($"Warning MachineTargetValues not assigned on lookup. MachineID:{_MachineID}");
              break;
            }

            /* TODO: Validate machine scope context for the UseMachineRMVThreshold and OverrideRMVJumpThreshold ie: Is it really a single value per machine configuration...
            if (TrackingState.MachineTargetValues.Machine != null)
            with TICMachine(MachineTargetValues.Machine) do
              {
                TrackingState.TrackingUseMachineRMVThreshold = UseMachineRMVThreshold;
                TrackingState.TrackingOverrideRMVJumpThreshold = OverrideRMVJumpThreshold;
              }  
            */
          }          
        }

        if (TrackingState.MachineTargetValues == null)
          return;

        passes[I].MachineType = SiteModel.Machines[TrackingState.MachineTargetValues.MachineID].MachineType;

        // ******************************************************
        // ****************** Target Values *********************
        // ******************************************************

        /* Long hand implementation for CCV values, replaced by DetermineTrackingStateValue in the version below
        if (populationControl.WantsTargetCCVValues)
        {
          if (TrackingState.TargetCCV_Tracking.Stamp == Stamp)
          {
            if (_Time >= TrackingState.TargetCCV_Tracking.EndDate)
            {
              if (TrackingState.TargetCCV_Tracking.IsNextEventSuitable(Stamp, _Time, TrackingState.MachineTargetValues.TargetCCVStateEvents))
                TrackingState.TargetCCV = TrackingState.TargetCCV_Tracking.ThisEvent;
              else
              {
                TrackingState.TargetCCV = TrackingState.MachineTargetValues.TargetCCVStateEvents.GetValueAtDate(_Time, out int _);
                TrackingState.TargetCCV_Tracking.RecordEventState(Stamp, TrackingState.MachineTargetValues.TargetCCVStateEvents);
              }
            }
          }
          else if (TrackingState.TargetCCV_Tracking.IsCurrentEventSuitable(_Time))
            TrackingState.TargetCCV_Tracking.Stamp = Stamp;
          else
          {
            TrackingState.TargetCCV = TrackingState.MachineTargetValues.TargetCCVStateEvents.GetValueAtDate(_Time, out int _);
            TrackingState.TargetCCV_Tracking.RecordEventState(Stamp, TrackingState.MachineTargetValues.TargetCCVStateEvents);
          }

          passes[I].TargetValues.TargetCCV = TrackingState.TargetCCV;
        }
        */

        if (populationControl.WantsTargetCCVValues)
        {
          TrackingState.TargetCCV = TrackingState.TargetCCV_Tracking.DetermineTrackingStateValue(Stamp, _Time,
            TrackingState.MachineTargetValues.TargetCCVStateEvents);
          passes[I].TargetValues.TargetCCV = TrackingState.TargetCCV;
        }

        if (populationControl.WantsTargetMDPValues)
        {
          TrackingState.TargetMDP = TrackingState.TargetMDP_Tracking.DetermineTrackingStateValue(Stamp, _Time,
            TrackingState.MachineTargetValues.TargetMDPStateEvents);
          passes[I].TargetValues.TargetMDP = TrackingState.TargetMDP;
        }

        if (populationControl.WantsTargetCCAValues)
        {
          TrackingState.TargetCCA = TrackingState.TargetCCA_Tracking.DetermineTrackingStateValue(Stamp, _Time,
            TrackingState.MachineTargetValues.TargetCCAStateEvents);
          passes[I].TargetValues.TargetCCA = TrackingState.TargetCCA;
        }

        if (populationControl.WantsTargetPassCountValues)
        {
          TrackingState.TargetPassCount = TrackingState.TargetPassCount_Tracking.DetermineTrackingStateValue(Stamp,
            _Time, TrackingState.MachineTargetValues.TargetPassCountStateEvents);
          passes[I].TargetValues.TargetPassCount = TrackingState.TargetPassCount;
        }

        if (populationControl.WantsTargetLiftThicknessValues)
        {
          TrackingState.TargetLiftThickness =
            TrackingState.TargetLiftThickness_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.TargetLiftThicknessStateEvents);
          passes[I].TargetValues.TargetLiftThickness = TrackingState.TargetLiftThickness;
        }

        if (populationControl.WantsTempWarningLevelMinValues)
        {
          TrackingState.TempWarningLevelMin =
            TrackingState.TempWarningLevelMin_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.TargetMinMaterialTemperature);
          passes[I].TargetValues.TempWarningLevelMin = TrackingState.TempWarningLevelMin;
        }

        if (populationControl.WantsTempWarningLevelMaxValues)
        {
          TrackingState.TempWarningLevelMax =
            TrackingState.TempWarningLevelMax_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.TargetMaxMaterialTemperature);
          passes[I].TargetValues.TempWarningLevelMax = TrackingState.TempWarningLevelMax;
        }

        // *****************************************************
        // ****************** Event Values *********************
        // *****************************************************

        if (populationControl.WantsEventDesignNameValues)
        {
          TrackingState.EventDesignNameID =
            TrackingState.EventDesignNameID_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.DesignNameIDStateEvents);
          passes[I].EventValues.EventDesignNameID = TrackingState.EventDesignNameID;
        }

        if (populationControl.WantsEventVibrationStateValues)
        {
          TrackingState.EventVibrationState =
            TrackingState.EventVibrationState_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.VibrationStateEvents);
          passes[I].EventValues.EventVibrationState = TrackingState.EventVibrationState;

          if (!ignoreBussinessRulesRules && passes[I].EventValues.EventVibrationState != VibrationState.On)
            passes[I].FilteredPass.SetFieldsForVibeStateOff();
        }

        if (populationControl.WantsEventAutoVibrationStateValues)
        {
          TrackingState.EventAutoVibrationState =
            TrackingState.EventAutoVibrationState_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.AutoVibrationStateEvents);
          passes[I].EventValues.EventAutoVibrationState = TrackingState.EventAutoVibrationState;
        }

        if (populationControl.WantsEventMinElevMappingValues)
        {
          TrackingState.MinElevMappingState =
            TrackingState.MinElevMappingState_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.MinElevMappingStateEvents);
          passes[I].EventValues.EventMinElevMapping = TrackingState.MinElevMappingState;
        }

        if (populationControl.WantsEventGPSAccuracyValues)
        {
          TrackingState.GPSAccuracyAndTolerance =
            TrackingState.GPSAccuracyState_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.GPSAccuracyAndToleranceStateEvents);
          passes[I].EventValues.GPSAccuracy = TrackingState.GPSAccuracyAndTolerance.GPSAccuracy;
          passes[I].EventValues.GPSTolerance = TrackingState.GPSAccuracyAndTolerance.GPSTolerance;
        }

        if (populationControl.WantsEventPositioningTechValues)
        {
          TrackingState.PositioningTechState =
            TrackingState.PositioningTechState_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.PositioningTechStateEvents);
          passes[I].EventValues.PositioningTechnology = TrackingState.PositioningTechState;
        }

        if (populationControl.WantsEventICFlagsValues)
        {
          TrackingState.EventICFlag =
            TrackingState.EventICFlag_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.ICFlagsStateEvents);
          passes[I].EventValues.EventFlags = TrackingState.EventICFlag;
        }

        if (populationControl.WantsEventMachineGearValues)
        {
          TrackingState.EventMachineGear = TrackingState.EventMachineGear_Tracking.DetermineTrackingStateValue(Stamp,
            _Time, TrackingState.MachineTargetValues.MachineGearStateEvents);
          passes[I].EventValues.EventMachineGear = TrackingState.EventMachineGear;
        }

        if (populationControl.WantsEventMachineCompactionRMVJumpThreshold)
        {
          if (TrackingState.TrackingUseMachineRMVThreshold)
          {
            TrackingState.EventMachineRMVThreshold =
              TrackingState.EventMachineRMVThreshold_Tracking.DetermineTrackingStateValue(Stamp, _Time,
                TrackingState.MachineTargetValues.RMVJumpThresholdEvents);
            passes[I].EventValues.EventMachineRMVThreshold = TrackingState.EventMachineRMVThreshold;
          }
          else
            passes[I].EventValues.EventMachineRMVThreshold = TrackingState.TrackingOverrideRMVJumpThreshold;
        }

        if (populationControl.WantsEventMachineAutomaticsValues)
        {
          TrackingState.EventMachineAutomatics =
            TrackingState.EventMachineAutomatics_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.MachineAutomaticsStateEvents);
          passes[I].EventValues.EventMachineAutomatics = TrackingState.EventMachineAutomatics;
        }

        if (populationControl.WantsLayerIDValues)
        {
          TrackingState.EventLayerID =
            TrackingState.EventLayerID_Tracking.DetermineTrackingStateValue(Stamp, _Time,
              TrackingState.MachineTargetValues.LayerIDStateEvents);
          passes[I].EventValues.LayerID = TrackingState.EventLayerID;
        }

        /*
        if WantsEventMapResetValues then
         begin
           with EventMapReset_Tracking do
             if Stamp = FStamp then
               begin
                 if (_Time >= EndDate) then
                   if IsNextEventSuitable(MapResets, EventMapReset_Tracking) then
                     begin
                       with TICEventMapReset(ThisEvent) do
                         begin
                           EventMapResetPriorDate := EventDate;
                           EventMapResetDesignID := DesignNameID;
                         end;
                     end
                   else
                     begin
                       EventMapResetPriorDate := MapResets.GetLastMapResetPriorTo(_Time, Index, EventMapResetDesignID);
                       RecordEventState(MapResets, EventMapReset_Tracking);
                     end;
               end
             else
               if IsCurrentEventSuitable(EventMapReset_Tracking) then
                 Stamp := FStamp
               else
                 begin
                   EventMapResetPriorDate := MapResets.GetLastMapResetPriorTo(_Time, Index, EventMapResetDesignID);
                   RecordEventState(MapResets, EventMapReset_Tracking);
                 end;

           EventValues.MapReset_PriorDate    := EventMapResetPriorDate;
           EventValues.MapReset_DesignNameID := EventMapResetDesignID;
         end;
        */

        I += ProcessInForwardDirection ? 1 : -1;
      } while (I != TerminationIndex);
    }
  }
}
