using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Swather;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.Processors
{
    /// <summary>
    /// Coordinates reading and converting recorded information from compaction machines into the IC server database.
    /// </summary>
    public class TAGProcessor : TAGProcessorBase
    {  
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGProcessor>();

        public TAGProcessor()
        {
        }

        /// <summary>
        /// Primary constructor for the TAGProcessor. The arguments passed to it are:
        /// 1. The target SiteModel which is intended to be the recipient of the TAG information processed 
        /// 2. The target Machine in the site model which recorded the TAG information
        /// 3. The event lists related to the target machine in the target site model
        /// 4. A subgrid tree representing the aggregator for all the spatial cell pass infromation processed
        ///    from the TAG information as an independent entity.
        /// 5. A set of event lists representing the aggregator for all the machine events for the target machine
        ///    in the target site model that were processed from the TAG information as a separate entity.
        /// </summary>
        /// <param name="targetSiteModel"></param>
        /// <param name="targetMachine"></param>
        /// <param name="siteModelGridAggregator"></param>
        /// <param name="machineTargetValueChangesAggregator"></param>
        public TAGProcessor(ISiteModel targetSiteModel,
            IMachine targetMachine,
            IServerSubGridTree siteModelGridAggregator,
            ProductionEventLists machineTargetValueChangesAggregator) : this()
        {
            SiteModel = targetSiteModel;
            Machine = targetMachine;

            SiteModelGridAggregator = siteModelGridAggregator;
            MachineTargetValueChangesAggregator = machineTargetValueChangesAggregator;
            //            MachineTargetValueChangesAggregator.MarkAllEventListsAsInMemoryOnly;
        }

        // SiteModel is the site model that the read data is being contributed to
        private ISiteModel SiteModel;

        // SiteModelAggregator is the site model that the read data is aggregated into
        // prior to being integrated into the model represented by SiteModel
        // This serves two functions:
        // 1. Performance: Cell passes and events are added to the relevant stores en-masse which
        //    is much faster than in piecemeal fashion.
        // 2. Contention: This reduces contention for the primary server interface lock between
        //    the tag file processor and client applications. }
        public IServerSubGridTree SiteModelGridAggregator { get; set; }

        // Machine is a reference to the intelligent compaction machine that
        // has collected the data being processed.
        private IMachine Machine;

        // FICMachineTargetValueChangesAggregator is an object that aggregates all the
        // machine state events of interest that we encounter while processing the
        // file. These are then integrated into the machine events in a single step
        // at a later point in processing
        public ProductionEventLists MachineTargetValueChangesAggregator { get; set; }

        /*
        // FOnProgressCheck provides a callback to the owner of the ST processing
        // currently underway. The owner may abort the processing by returning
        // false when the event is called.
        FOnProgressCheck : TSTProcessingProgressCheckEvent;

        // FOnAbortProcessing is an event that allows the processor to advise a third
        // party that the processing had been aborted.
        FOnAbortProcessing : TNotifyEvent; 
        */

        private DateTime TagFileStartTime = DateTime.MinValue;
        private bool HasGPSModeBeenSet = false;

        /// <summary>
        /// EpochContainsProofingRunDescription determines if the current epoch
        /// contains a description of a proofing run.
        /// </summary>
        protected bool EpochContainsProofingRunDescription()
        {
            return (StartProofingDataTime != DateTime.MinValue) && // We have a start time for the run
                   (DataTime > StartProofingDataTime); // The current epoch time is greater than the start
        }

        // If there has been sufficient information read in from the compaction
        // information file to identify a proofing pass made by the machine then
        // we process it here.
        protected bool ProcessProofingPassInformation()
        {
            bool Result = true;
            string TempStr = EndProofingName != string.Empty ? EndProofingName :
                Design == string.Empty ? "No Design" : Design;

            DateTime LocalTime = StartProofingDataTime + Time.GPS.GetLocalGMTOffset();

            EndProofingName = $"{TempStr} ({LocalTime:YYYY:MM:DD} {LocalTime:HH:mm:ss})";

/* TODO add when proofing runs are supported
            // Create a new proofing run entry to represent this run
            int ExistingIndex = SiteModel.ProofingRuns.IndexOf(EndProofingName,
                                                               Machine.ID,
                                                               StartProofingDataTime,
                                                               DataTime);
            if (ExistingIndex == -1)
            {
                // no match found - create a new one
                Result = SiteModel.ProofingRuns.CreateNew(EndProofingName,
                                                              Machine.ID,
                                                              StartProofingDataTime,
                                                              DataTime,
                                                              ProofingRunExtent) != null;
            }
            else
            {
                // We found an exact match. No need to add this proofing run again
            }
*/
            ProofingRunExtent.SetInverted();

            return Result;
        }

        /// <summary>
        /// At every epoch we process a set of state information that has been set into
        /// this processor by the active tag file value sink. Most of this information
        /// persists between epochs. However, some of this information is cleared at
        /// the end of each epoch. ClearEpochSpecificData performs this operation
        /// and is called at the end of ProcessEpochContext()
        /// </summary>
        protected override void ClearEpochSpecificData()
        {
            base.ClearEpochSpecificData();
        }

        private SwatherBase CreateSwather(Fence InterpolationFence)
        {
            // Decide which swather to create. Currently it's just a standard terrain swather
            return new TerrainSwather(this,
                MachineTargetValueChangesAggregator,
                SiteModel,
                SiteModelGridAggregator,
                //Machine.ID,
                Machine.InternalSiteModelMachineIndex,
                //     FICMachine.ConnectedMachineLevel,
                InterpolationFence)
            {
                ProcessedEpochNumber = ProcessedEpochCount
            };
        }

        protected override void SetDataTime(DateTime Value)
        {
            bool RecordEvent = DataTime == DateTime.MinValue;

            base.SetDataTime(Value);

            if (RecordEvent)
            {
                MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.PutValueAtDate(Value,
                    ProductionEventType.StartEvent);

                TagFileStartTime = Value;
            }
        }

        protected override void SetDesign(string Value)
        {
            // If the design being loaded changed, then update the extents of the design
            // in the designs list in the sitemodel

            if (Design != "" && Design != Value)
                UpdateCurrentDesignExtent();

            base.SetDesign(Value);

            if (DataTime != DateTime.MinValue)
            {
        // TODO: Add the design, a bit like this in Raptor:
        /*
function TICProductionEventChanges.AddDesignChangeEvent(const ADate: TICPassTime;
                                                  DesignName: TICDesignName): TICEventDesignNameValueChange;
var
Index : integer;
DesignNameID : TICDesignNameID;
NewDesignName : TEventDesignName;
begin
if strip_blanks(DesignName) = '' then
DesignName := kNoDesignName;

with TICSiteModel(FSiteModel) do
if SiteModelDesignNames.Find_WideString(DesignName, Index) then
DesignNameID := SiteModelDesignNames[Index].ID
else
begin
  NewDesignName := SiteModelDesignNames.AddDesignName(DesignName);
  DesignNameID := NewDesignName.ID;
end;
end;               */

        //   MachineTargetValueChangesAggregator.DesignNameStateEvents.PutValueAtDate(DataTime, DesignNameID);
      }
      else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //Log.LogDebug("DataTime = 0 in SetDesign");
                //{$ENDIF}
            }

            // Get the current design extent for the newly selected design
            SelectCurrentDesignExtent();
        }

        protected void SelectCurrentDesignExtent()
        {
            //TODO: FICSiteModel.SiteModelDesigns.AcquireLock;
            try
            {
                int DesignIndex = SiteModel.SiteModelDesigns.IndexOf(Design);

                if (DesignIndex == -1)
                {
// This may be because there is no selected design name, of that the
// entry for this named design is not in the list. If the former, just clear the
// design extents. If the latter, create a new design extents entry

                    // Clear the design extent being maintained in the processor.
                    DesignExtent.SetInverted();

                    if (Design != "")
                        SiteModel.SiteModelDesigns.CreateNew(Design, DesignExtent);
                }
                else
                    DesignExtent = SiteModel.SiteModelDesigns[DesignIndex].Extents;
            }
            finally
            {
                //TODO - FICSiteModel.SiteModelDesigns.ReleaseLock;
            }
        }

        /// <summary>
        /// Records a change in the 'ICMode' flags from the compaction system. These flags also drive two 
        /// other events: vibration events and automatics vibration events
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICMode(byte Value)
        {
            base.SetICMode(Value);

            VibrationState TempVibrationState = VibrationState.Invalid ;
            AutoVibrationState TempAutoVibrationState = AutoVibrationState.Unknown;

            if (DataTime != DateTime.MinValue)
            {
                switch (ICSensorType)
                {
                    case CompactionSensorType.Volkel:
                    {
                        if ((((Value & 0x01) >> 0) != 0) &&
                            (((Value & 0x02) >> 1) != 0) &&
                            (((Value & 0x04) >> 2) != 0) &&
                            (((Value & 0x08) >> 3)) != 0) // Vibration is On...
                            TempVibrationState = VibrationState.On;
                        else // Vibration is Off...
                            TempVibrationState = VibrationState.Off;

                        TempAutoVibrationState = AutoVibrationState.Unknown;

                        break;
                    }

                    case CompactionSensorType.MC024:
                    case CompactionSensorType.CATFactoryFitSensor:
                    {

                        TempVibrationState = (VibrationState) ((Value & 0x04) >> 2);
                        TempAutoVibrationState = (AutoVibrationState) (Value & 0x03);
                        break;
                    }

                    case CompactionSensorType.NoSensor:

                    {
                        // Per TFS US 37212: Machines that do not report a compaction sensor type will
                        // report vibration state information directly from the machine ECM in the FLAGS TAG.
                        TempVibrationState = (VibrationState) ((Value & 0x04) >> 2);
                        TempAutoVibrationState = (AutoVibrationState) (Value & 0x03);
                        break;
                    }
                    default:
                        Debug.Assert(false, "Unknown sensor type");
                        break;
                }

                MachineTargetValueChangesAggregator.VibrationStateEvents.PutValueAtDate(DataTime, TempVibrationState);
                MachineTargetValueChangesAggregator.AutoVibrationStateEvents.PutValueAtDate(DataTime, TempAutoVibrationState);
                MachineTargetValueChangesAggregator.ICFlagsStateEvents.PutValueAtDate(DataTime, Value);
            }
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICMode',slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Adds the CCV target value set on the machine into the target CCV list
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICCCVTargetValue(short Value)
        {
            base.SetICCCVTargetValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.TargetCCVStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICCCVTargetValue', slpmcDebug); 
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Adds the CCA target value set on the machine into the target CCA list
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICCCATargetValue(byte Value)
        {
            base.SetICCCATargetValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.TargetCCAStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICCCATargetValue', slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Adds the MDP target value set on the machine into the target MDP list
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICMDPTargetValue(short Value)
        {
            base.SetICMDPTargetValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.TargetMDPStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICMDPTargetValue', slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Adds the MDP target value set on the machine into the target MDP list
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICPassTargetValue(ushort Value)
        {
            base.SetICPassTargetValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.TargetPassCountStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICPassTargetValue', slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Converts the machine direction indicated by Value into a forwards or reverse gear, and injects it into the machine gear events list
        /// </summary>
        /// <param name="Value"></param>
        public override void SetMachineDirection(MachineDirection Value)
        {
            base.SetMachineDirection(Value);

            if (GearValueReceived)
                return;

            MachineGear Gear = MachineGear.Null;

            if (Value == TRex.Types.MachineDirection.Forward)
                Gear = MachineGear.Forward;
            else if (Value == TRex.Types.MachineDirection.Reverse)
                Gear = MachineGear.Reverse;

            if (DataTime != DateTime.MinValue && (Gear == MachineGear.Forward || Gear == MachineGear.Reverse))
                MachineTargetValueChangesAggregator.MachineGearStateEvents.PutValueAtDate(DataTime, Gear);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 or Gear not Forward/Reverse in SetMachineDirection', slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Sets the machine gear into the machine gear events list
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICGear(MachineGear Value)
        {
            base.SetICGear(Value);

            if (DataTime != DateTime.MinValue && Value != MachineGear.SensorFailedDeprecated)
                MachineTargetValueChangesAggregator.MachineGearStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICGear', slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Sets the target minimum material temperture into the machine target material temperature events list
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICTempWarningLevelMinValue(ushort Value)
        {
            base.SetICTempWarningLevelMinValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.TargetMinMaterialTemperature.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICTempWarningLevelMinValue', slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Sets the target maximum material temperture into the machine target material temperature events list
        /// </summary>
        /// <param name="Value"></param>

        protected override void SetICTempWarningLevelMaxValue(ushort Value)
        {
            base.SetICTempWarningLevelMaxValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.TargetMaxMaterialTemperature.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICTempWarningLevelMaxValue', slpmcDebug);
                //{$ENDIF}
            }
        }

        /// <summary>
        /// Sets the target lift thickness into the machine target lift thickness events list
        /// </summary>
        /// <param name="Value"></param>
        protected override void SetICTargetLiftThickness(float Value)
        {
            base.SetICTargetLiftThickness(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.TargetLiftThicknessStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICTargetLiftThickness', slpmcDebug);
                //{$ENDIF}
            }
        }

        public override void SetICCCVValue(short Value)
        {
            base.SetICCCVValue(Value);
            Machine.CompactionDataReported = true;
        }

        public override void SetICRMVValue(short Value)
        {
            base.SetICRMVValue(Value);
            Machine.CompactionDataReported = true;
        }

        public override void SetICMDPValue(short Value)
        {
            base.SetICMDPValue(Value);
            Machine.CompactionDataReported = true;
        }

        public override void SetICCCAValue(byte Value)
        {
            base.SetICCCAValue(Value);
            Machine.CompactionDataReported = true;
        }

        public override void SetICCCALeftFrontValue(byte Value)
        {
            base.SetICCCALeftFrontValue(Value);
            Machine.CompactionDataReported = true;
        }

        public override void SetICCCARightFrontValue(byte Value)
        {
            base.SetICCCARightFrontValue(Value);
            Machine.CompactionDataReported = true;
        }

        public override void SetICCCALeftRearValue(byte Value)
        {
            base.SetICCCALeftRearValue(Value);
            Machine.CompactionDataReported = true;
        }

        public override void SetICCCARightRearValue(byte Value)
        {
            base.SetICCCARightRearValue(Value);
            Machine.CompactionDataReported = true;
        }

        protected override void SetRMVJumpThresholdValue(short Value)
        {
            base.SetRMVJumpThresholdValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.RMVJumpThresholdEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetRMVJumpThresholdValue', slpmcDebug);
                //{$ENDIF}
            }
        }

        protected override void SetICSensorType(CompactionSensorType Value)
        {
            base.SetICSensorType(Value);

            if (DataTime != DateTime.MinValue)
            {
                // Tell the machine object itself what the current sensor type is
                Machine.CompactionSensorType = Value;
            }
            else
            {
                // {$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICSensorType', slpmcDebug);
                //{$ENDIF}
            }
        }

        protected override void SetAutomaticsMode(MachineAutomaticsMode Value)
        {
            base.SetAutomaticsMode(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.MachineAutomaticsStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetAutomaticsMode', slpmcDebug);
                //{$ENDIF}
            }
        }

        protected override void SetICLayerIDValue(ushort Value)
        {
            base.SetICLayerIDValue(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.LayerIDStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetICLayerIDValue', slpmcDebug);
                //{$ENDIF}
            }
        }

        public override void SetGPSMode(GPSMode Value)
        {
            base.SetGPSMode(Value);

            if (DataTime != DateTime.MinValue)
            {
                if (PositioningTech == TRex.Types.PositioningTech.Unknown || PositioningTech == TRex.Types.PositioningTech.UTS)
                {
                    if (Value != GPSMode.NoGPS)
                        MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(DataTime, TRex.Types.PositioningTech.GPS);
                    else
                        MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(DataTime, TRex.Types.PositioningTech.UTS);
                }

                MachineTargetValueChangesAggregator.GPSModeStateEvents.PutValueAtDate(DataTime, Value);

                HasGPSModeBeenSet = true;
            }
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetGPSMode', slpmcDebug);
                //{$ENDIF}
            }
        }

        public override void SetMinElevMappingState(bool Value)
        {
            base.SetMinElevMappingState(Value);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.MinElevMappingStateEvents.PutValueAtDate(DataTime, Value);
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetMinElevMappingState', slpmcDebug);
                //{$ENDIF}
            }
        }

        public override void SetGPSAccuracyState(GPSAccuracy accuracy, ushort tolerance)
        {
            base.SetGPSAccuracyState(accuracy, tolerance);

            if (DataTime != DateTime.MinValue)
                MachineTargetValueChangesAggregator.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(DataTime, new GPSAccuracyAndTolerance(accuracy, tolerance));
            else
            {
                //{$IFDEF DENSE_TAG_FILE_LOGGING}
                //SIGLogProcessMessage.Publish(Self, 'DataTime = 0 in SetGPSAccuracyState', slpmcDebug);
                //{$ENDIF}
            }
        }

        /*
      
       function MaxEpochInterval: Double; override;
       function IgnoreInvalidPositions: Boolean; override;

       procedure SetICSonic3D                  (const Value :Byte                  ); override;
       procedure SetInAvoidZoneState(const Value: TICInAvoidZoneState); override;
       procedure SetAgeOfCorrection(const Value: Byte); override;
       */

        /// <summary>
        /// Updates the bounding box surrounding the area of the project worked on with the current
        /// design name selected on the machine.
        /// </summary>
        protected void UpdateCurrentDesignExtent()
          {
              lock (SiteModel.SiteModelDesigns)
              {
                  int DesignIndex = SiteModel.SiteModelDesigns.IndexOf(Design);

                  if (DesignIndex != -1)
                  {
                      SiteModel.SiteModelDesigns[DesignIndex].Extents = DesignExtent;
                  }

                  // Clear the design extent being maintained in the processor.
                  DesignExtent.SetInverted();
              }
          }

        /// <summary>
        /// DoProcessEpochContext is the method that does the actual processing
        /// of the epoch intervals into the appropriate data structures. Descendant
        /// classes must override this function.
        /// </summary>
        /// <param name="InterpolationFence"></param>
        /// <param name="machineSide"></param>
        /// <returns></returns>
        public override bool DoProcessEpochContext(Fence InterpolationFence, MachineSide machineSide )
        {
            Debug.Assert(SiteModel != null, "Null site model/data store for processor");
            Debug.Assert(Machine != null, "Null machine reference for processor");

            SwatherBase Swather = CreateSwather(InterpolationFence);

            if (Swather == null)
            {
                Debug.Assert(false, "Unable to create appropriate swather for processing epoch");
                return false;
            }

            // Primary e.g. blade, front drum
            Swather.PerformSwathing(FrontHeightInterpolator1, FrontHeightInterpolator2, FrontTimeInterpolator1,
                                    FrontTimeInterpolator2, HasRearAxleInThisEpoch, PassType.Front, machineSide);

            // rear positions
            if (HasRearAxleInThisEpoch)
            {
                Swather.PerformSwathing(RearHeightInterpolator1, RearHeightInterpolator2, RearTimeInterpolator1,
                                        RearTimeInterpolator2, HasRearAxleInThisEpoch, PassType.Rear, machineSide);
            }

            // track positions
            if (HasTrackInThisEpoch)
            {
                Swather.PerformSwathing(TrackHeightInterpolator1, TrackHeightInterpolator2, TrackTimeInterpolator1,
                                        TrackTimeInterpolator2, false, PassType.Track, machineSide);
            }

            // wheel positions
            if (HasWheelInThisEpoch)
            {
                Swather.PerformSwathing(WheelHeightInterpolator1, WheelHeightInterpolator2, WheelTimeInterpolator1,
                                        WheelTimeInterpolator2, false, PassType.Wheel, machineSide);
            }

            return true;
        }

        /// <summary>
        /// DoPostProcessFileAction is called immediately after the file has been
        /// processed. It allows a descendent class to implement appropriate actions
        /// such as saving data when the reading process is complete.
        /// SuccessState reflects the success or failure of the file processing.
        /// </summary>
        /// <param name="successState"></param>
        public override void DoPostProcessFileAction(bool successState)
        {
            // Record the last data time as the data end event
            if (DataTime != DateTime.MinValue)
            {
                MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.PutValueAtDate(DataTime, ProductionEventType.EndEvent);

                if (!HasGPSModeBeenSet)
                {
                    MachineTargetValueChangesAggregator.GPSModeStateEvents.PutValueAtDate(TagFileStartTime, GPSMode.NoGPS);
                    MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(TagFileStartTime, TRex.Types.PositioningTech.UTS);
                }
            }

            // Take into account the fact that the site model extent computed from TAG file swathing
            // operations bounds the cell center points and does not take into account that cells have
            // an area. Expand the computed site model extent by half a cell size to ensure the reported
            // site model extent covers the extent of the cells created by swathing the TAG file.
            SiteModel.SiteModelExtent.Expand(SiteModelGridAggregator.CellSize / 2, SiteModelGridAggregator.CellSize / 2);

            // Update the design extent...
            if (Design != string.Empty)
            {
                // TODO readd when designs are implemented
                // UpdateCurrentDesignExtent;
            }
        }

        /// <summary>
        /// Handles a specific set of events that cause modifications to the epoch state, such as positioning
        /// technology, machine start/stop events and map resets
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public override bool DoEpochStateEvent(EpochStateEvent eventType)
        {
            switch (eventType)
            {
                case EpochStateEvent.Unknown:
                  Debug.Assert(false, "Unknown epoch state event type");
                  break;

                case EpochStateEvent.MachineStartup:
                  if (DataTime != DateTime.MinValue)
                    MachineTargetValueChangesAggregator.MachineStartupShutdownEvents.PutValueAtDate(DataTime, ProductionEventType.StartEvent);
                  break;

                case EpochStateEvent.MachineShutdown:
                  if (DataTime != DateTime.MinValue)
                    MachineTargetValueChangesAggregator.MachineStartupShutdownEvents.PutValueAtDate(DataTime, ProductionEventType.EndEvent);
                  break;

                case EpochStateEvent.MachineMapReset:
                  // Todo: Map reset events not implemented yet
                  //if (DataTime != DateTime.MinValue)
                  //MachineTargetValueChangesAggregator.MapResetEvents.PutValueAtDate(DataTime, Design);
                  break;

                case EpochStateEvent.MachineInUTSMode:
                  if (DataTime != DateTime.MinValue)
                  {
                    PositioningTech = PositioningTech.UTS;
                    MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(DataTime, PositioningTech);
                  }               
                  break;

                default:
                  throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

          return true;
        }
     
        /// <summary>
        /// DoEpochPreProcessAction is called in ProcessEpochContext immediately
        /// before any processing of the epoch information is done. It allows a
        /// descendent class to implement appropriate actions such as inspecting
        /// or processing other information in the epoch not direclty related
        /// to the epoch interval itself (such as proofing run information in
        /// intelligent compaction tag files.
        /// </summary>
        /// <returns></returns>
        public override bool DoEpochPreProcessAction()
        {
            return !EpochContainsProofingRunDescription() || ProcessProofingPassInformation();
        }
    }
}
