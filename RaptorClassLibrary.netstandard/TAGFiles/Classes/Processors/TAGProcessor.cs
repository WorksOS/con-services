using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Swather;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes
{
    /// <summary>
    /// Coordinates reading and converting recorded information from compaction machines into the IC server database.
    /// </summary>
    public class TAGProcessor : TAGProcessorBase
    {
        public TAGProcessor() : base()
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
        /// <param name="targetValues"></param>
        /// <param name="siteModelGridAggregator"></param>
        /// <param name="machineTargetValueChangesAggregator"></param>
        public TAGProcessor(SiteModel targetSiteModel, 
            Machine targetMachine, 
            ProductionEventChanges targetValues,
            ServerSubGridTree siteModelGridAggregator, 
            ProductionEventChanges machineTargetValueChangesAggregator) : this()
        {
            SiteModel = targetSiteModel;
            Machine = targetMachine;
            MachineTargetValueChanges = targetValues;

            SiteModelGridAggregator = siteModelGridAggregator;
            MachineTargetValueChangesAggregator = machineTargetValueChangesAggregator;
            //            MachineTargetValueChangesAggregator.MarkAllEventListsAsInMemoryOnly;
        }

        // SiteModel is the site model that the read data is being contributed to
        private SiteModel SiteModel = null;

        // SiteModelAggregator is the site model that the read data is aggregated into
        // prior to being integrated into the model represented by SiteModel
        // This serves two functions:
        // 1. Performance: Cell passes and events are added to the relevant stores en-masse which
        //    is much faster than in piecemeal fashion.
        // 2. Contention: This reduces contention for the primary server interface lock between
        //    the tag file processor and client applications. }
        public ServerSubGridTree SiteModelGridAggregator { get; set; } = null;

        // Machine is a reference to the intelligent compaction machine that
        // has collected the data being processed.
        private Machine Machine = null;

        // MachineTargetValueChanges is a reference to an object that records all the
        // machine state events of interest that we encounter while processing the
        // file
        private ProductionEventChanges MachineTargetValueChanges = null;

        // FICMachineTargetValueChangesAggregator is an object that aggregates all the
        // machine state events of interest that we encounter while processing the
        // file. These are then integrated into the machine events in a single step
        // at a later point in processing
        public ProductionEventChanges MachineTargetValueChangesAggregator { get; set; } = null;

        /*
         *      // FOnProgressCheck provides a callback to the owner of the ST processing
              // currently underway. The owner may abort the processing by returning
              // false when the event is called.
              FOnProgressCheck : TSTProcessingProgressCheckEvent;

              // FOnAbortProcessing is an event that allows the processor to advise a third
              // party that the processing had been aborted.
              FOnAbortProcessing : TNotifyEvent; */

        private DateTime TagFileStartTime = DateTime.MinValue;
        private bool HasGPSModeBeenSet = false;

        /// <summary>
        /// EpochContainsProofingRunDescription determines if the current epoch
        /// contains a description of a proofing run.
        /// </summary>
        protected bool EpochContainsProofingRunDescription()
        {
            return (StartProofingDataTime != DateTime.MinValue) &&  // We have a start time for the run
                   (DataTime > StartProofingDataTime); // The current epoch time is greater than the start
        }

        // If there has been sufficient information read in from the compaction
        // information file to identify a proofing pass made by the machine then
        // we process it here.
        protected bool ProcessProofingPassInformation()
        {
            bool Result = true;
            string TempStr = EndProofingName != string.Empty ? EndProofingName : Design == string.Empty ? "No Design" : Design;

            DateTime LocalTime = StartProofingDataTime + Time.GPS.GetLocalGMTOffset();

            EndProofingName = string.Format("{0} ({1:YYYY:MM:DD} {1:HH:mm:ss})", TempStr, LocalTime);

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
            TerrainSwather Result = new TerrainSwather(this,
                                              MachineTargetValueChangesAggregator,
                                              SiteModel,
                                              SiteModelGridAggregator,
                                              Machine.ID,
                                         //     FICMachine.ConnectedMachineLevel,
                                              InterpolationFence);
            (Result as TerrainSwather).ProcessedEpochNumber = ProcessedEpochCount;

            return Result;
        }

        protected override void SetDataTime(DateTime Value)
        {
            bool RecordEvent = DataTime == DateTime.MinValue;

            base.SetDataTime(Value);

            if (RecordEvent)
            {
                MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.PutValueAtDate(Value, ProductionEventType.StartRecordedData);

                TagFileStartTime = Value;
            }
        }

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
        /// <returns></returns>
        public override bool DoProcessEpochContext(Fence InterpolationFence, MachineSide machineSide )
        {
            SwatherBase Swather;

            Debug.Assert(SiteModel != null, "Null site model/data store for processor");
            Debug.Assert(Machine != null, "Null machine reference for processor");

            Swather = CreateSwather(InterpolationFence);

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
                MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.PutValueAtDate(DataTime, ProductionEventType.EndRecordedData);

                if (!HasGPSModeBeenSet)
                {
                    MachineTargetValueChangesAggregator.GPSModeStateEvents.PutValueAtDate(TagFileStartTime, GPSMode.NoGPS);
                    MachineTargetValueChangesAggregator.PositioningTechStateEvents.PutValueAtDate(TagFileStartTime, PositioningTech.UTS);
                }
            }

            // Update the design extent...
            if (Design != string.Empty)
            {
                // TODO readd when designs are implemented
                // UpdateCurrentDesignExtent;
            }
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
            if (EpochContainsProofingRunDescription() && !ProcessProofingPassInformation())
            {
                return false;
            }

            return true;
        }
    }
}
