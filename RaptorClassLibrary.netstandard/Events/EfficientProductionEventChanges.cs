using System;
using System.Linq;
using System.Security.Permissions;
using VSS.VisionLink.Raptor.Events.Interfaces;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// A wrapper for all the event information related to a particular machine's activities within a particular
    /// site model.
    /// </summary>
    public class EfficientProductionEventChanges
    {
        /// <summary>
        /// The SiteModel these events relate to
        /// </summary>
        private SiteModel SiteModel { get; set; }

        /// <summary>
        /// The ID of the machine these events were recorded by
        /// </summary>
        public long MachineID { get; set; }

        /// <summary>
        /// Events recording the Start and Stop events for recording production data on a machine
        /// </summary>
        /// 
        public EfficientStartEndRecordedDataChangeList StartEndRecordedDataEvents;
        
        /// <summary>
        /// Events recording vibration state changes for vibratory drum compactor operation
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<VibrationState>, VibrationState> VibrationStateEvents;

        /// <summary>
        /// Events recording automatics vibration state changes for vibratory drum compactor operation
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<AutoVibrationState>, AutoVibrationState> AutoVibrationStateEvents;

        /// <summary>
        /// Events recording changes to the prevailing GPSMode (eg: RTK Fixed, RTK Float, Differential etc) at the time 
        /// production measurements were being made
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<GPSMode>, GPSMode> GPSModeStateEvents;

        /// <summary>
        /// Records the positioning technology (eg: GPS or UTS) being used at the time production measurements were being made
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<PositioningTech>, PositioningTech> PositioningTechStateEvents;

        /// <summary>
        /// Records the IDs of the designs selected on a machine at the time production measurements were being made
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<int>, int> DesignNameIDStateEvents;

        /// <summary>
        /// Records the state of the automatic machine control on the machine at the time measurements were being made.
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode> MachineAutomaticsStateEvents;

        /// <summary>
        /// Records the state of the selected machine gear at the time measurements were being made
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<MachineGear>, MachineGear> MachineGearStateEvents;

        /// <summary>
        /// Records the state of minimum elevation mapping on the machine at the time measurements were being made
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<bool>, bool> MinElevMappingStateEvents;

        /// <summary>
        /// Records the state of GPSAccuracy and accompanying GPSTolerance on the machine at the time measurements were being made
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance> GPSAccuracyAndToleranceStateEvents;

        /// <summary>
        /// Records the selected Layer ID on the machine at the time measurements were being made
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort> LayerIDStateEvents;

        /// <summary>
        /// Records the selected design on the machine at the time the measurements were made
        /// </summary>
//        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<string>, string> DesignNameStateEvents;

        /// <summary>
        /// ICFlags control flags change events
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<byte>, byte> ICFlagsStateEvents;

        /// <summary>
        /// Records the target CCV value configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short> TargetCCVStateEvents;

        /// <summary>
        /// Records the target CCA value configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short> TargetCCAStateEvents;

        /// <summary>
        /// Records the target MDP value configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short> TargetMDPStateEvents;

        /// <summary>
        /// Records the target MDP value configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort> TargetPassCountStateEvents;

        /// <summary>
        /// Records the target minimum temperature value configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort> TargetMinMaterialTemperature;

        /// <summary>
        /// Records the target maximum temperature value configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort> TargetMaxMaterialTemperature;

        /// <summary>
        /// Records the target lift thickness value configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<float>, float> TargetLiftThickness;

        /// <summary>
        /// Records the Resonance Meter Value jump threshold configured on the machine control system
        /// </summary>
        public EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short> RMVJumpThresholdEvents;
      
        /// <summary>
        /// Create all defined event lists in one operation.
        /// </summary>
        private void CreateEventLists()
        {
            StartEndRecordedDataEvents = new EfficientStartEndRecordedDataChangeList(this, MachineID, SiteModel.ID, ProductionEventType.StartEndRecordedData);
            VibrationStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<VibrationState>, VibrationState>(this, MachineID, SiteModel.ID, ProductionEventType.VibrationStateChange);
            AutoVibrationStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<AutoVibrationState>, AutoVibrationState>(this, MachineID, SiteModel.ID, ProductionEventType.AutoVibrationStateChange);
            GPSModeStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<GPSMode>, GPSMode>(this, MachineID, SiteModel.ID, ProductionEventType.GPSModeChange);
            PositioningTechStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<PositioningTech>, PositioningTech>(this, MachineID, SiteModel.ID, ProductionEventType.PositioningTech);

            DesignNameIDStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<int>, int>(this, MachineID, SiteModel.ID, ProductionEventType.DesignChange);
            MachineAutomaticsStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode>(this, MachineID, SiteModel.ID, ProductionEventType.MachineAutomaticsChange);
            MachineGearStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<MachineGear>, MachineGear>(this, MachineID, SiteModel.ID, ProductionEventType.MachineGearChange);
            MinElevMappingStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<bool>, bool>(this, MachineID, SiteModel.ID, ProductionEventType.MinElevMappingStateChange);
            GPSAccuracyAndToleranceStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance>(this, MachineID, SiteModel.ID, ProductionEventType.GPSAccuracyChange);
            LayerIDStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>(this, MachineID, SiteModel.ID, ProductionEventType.LayerID);
//            DesignNameStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<string>, string>(this, MachineID, SiteModel.ID, ProductionEventType.DesignChange);
            ICFlagsStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<byte>, byte>(this, MachineID, SiteModel.ID, ProductionEventType.ICFlagsChange);

            TargetCCVStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>(this, MachineID, SiteModel.ID, ProductionEventType.TargetCCV);
            TargetCCAStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>(this, MachineID, SiteModel.ID, ProductionEventType.TargetCCA);
            TargetMDPStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>(this, MachineID, SiteModel.ID, ProductionEventType.TargetMDP);
            TargetPassCountStateEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>(this, MachineID, SiteModel.ID, ProductionEventType.TargetPassCount);
            TargetMinMaterialTemperature = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>(this, MachineID, SiteModel.ID, ProductionEventType.TempWarningLevelMinChange);
            TargetMaxMaterialTemperature = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>(this, MachineID, SiteModel.ID, ProductionEventType.TempWarningLevelMaxChange);
            TargetLiftThickness = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<float>, float>(this, MachineID, SiteModel.ID, ProductionEventType.TargetLiftThickness);

            RMVJumpThresholdEvents = new EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>(this, MachineID, SiteModel.ID, ProductionEventType.MachineRMVJumpValueChange);
        }

        /// <summary>
        /// Returns an array containing all the event lists for a machine
        /// </summary>
        /// <returns></returns>
        public IProductionEventChangeList[] GetEventLists()
        {
            return new IProductionEventChangeList[]
            {
                StartEndRecordedDataEvents,
                VibrationStateEvents,
                AutoVibrationStateEvents,
                GPSModeStateEvents,
                PositioningTechStateEvents,
                DesignNameIDStateEvents,
                MachineAutomaticsStateEvents,
                MachineGearStateEvents,
                MinElevMappingStateEvents,
                GPSAccuracyAndToleranceStateEvents,
                LayerIDStateEvents,
//                DesignNameStateEvents,
                ICFlagsStateEvents,
                TargetCCVStateEvents,
                TargetCCAStateEvents,
                TargetMDPStateEvents,
                TargetPassCountStateEvents,
                TargetMinMaterialTemperature,
                TargetMaxMaterialTemperature,
                TargetLiftThickness,
                RMVJumpThresholdEvents
            };
        }
    /// <summary>
    /// Primary constructor for events recorded by a single machine within a single site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="machineID"></param>
    public EfficientProductionEventChanges(SiteModel siteModel, long machineID)
        {
            //  FFileSystem := Nil;
            SiteModel = siteModel;

            MachineID = machineID;

            CreateEventLists();
        }

        /// <summary>
        /// Saves the event lists for this machine to the persistent store
        /// </summary>
        /// <param name="storageProxy"></param>
        public void SaveMachineEventsToPersistentStore(IStorageProxy storageProxy)
        {
            foreach (IProductionEventChangeList list in GetEventLists())
            {
                list.SaveToStore(storageProxy);
            }
        }

        public bool LoadEventsForMachine(IStorageProxy storageProxy)
        {
            StartEndRecordedDataEvents = StartEndRecordedDataEvents.LoadFromStore(storageProxy) as EfficientStartEndRecordedDataChangeList;
            VibrationStateEvents = VibrationStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<VibrationState>, VibrationState>;
            AutoVibrationStateEvents = AutoVibrationStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<AutoVibrationState>, AutoVibrationState>;
            GPSModeStateEvents = GPSModeStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<GPSMode>, GPSMode>;
            PositioningTechStateEvents = PositioningTechStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<PositioningTech>, PositioningTech>;
            DesignNameIDStateEvents = DesignNameIDStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<int>, int>;
            MachineAutomaticsStateEvents = MachineAutomaticsStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode>;
            MachineGearStateEvents = MachineGearStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<MachineGear>, MachineGear>;
            MinElevMappingStateEvents = MinElevMappingStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<bool>, bool>;
            GPSAccuracyAndToleranceStateEvents = GPSAccuracyAndToleranceStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance>;
            LayerIDStateEvents = LayerIDStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>;
//            DesignNameStateEvents = DesignNameStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<string>, string>;
            ICFlagsStateEvents = ICFlagsStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<byte>, byte>;

            TargetCCVStateEvents = TargetCCVStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>;
            TargetCCAStateEvents = TargetCCAStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>;
            TargetMDPStateEvents = TargetMDPStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>;
            TargetPassCountStateEvents = TargetPassCountStateEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>;
            TargetMinMaterialTemperature = TargetMinMaterialTemperature.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>;
            TargetMaxMaterialTemperature = TargetMaxMaterialTemperature.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<ushort>, ushort>;
            TargetLiftThickness = TargetLiftThickness.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<float>, float>;

            RMVJumpThresholdEvents = RMVJumpThresholdEvents.LoadFromStore(storageProxy) as EfficientProductionEventChangeList<EfficientProductionEventChangeBase<short>, short>;

            // Wire the container (this object) into the list jsut read...
            GetEventLists().All(x =>
            {
                x.SetContainer(this);
                return true;
            });

            return true;
        }
    }
}
