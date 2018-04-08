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
    public class ProductionEventChanges
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
        public StartEndRecordedDataChangeList StartEndRecordedDataEvents;
        
        /// <summary>
        /// Events recording vibration state changes for vibratory drum compactor operation
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<VibrationState>, VibrationState> VibrationStateEvents;

        /// <summary>
        /// Events recording automatics vibration state changes for vibratory drum compactor operation
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<AutoVibrationState>, AutoVibrationState> AutoVibrationStateEvents;

        /// <summary>
        /// Events recording changes to the prevailing GPSMode (eg: RTK Fixed, RTK Float, Differential etc) at the time 
        /// production measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<GPSMode>, GPSMode> GPSModeStateEvents;

        /// <summary>
        /// Records the positioning technology (eg: GPS or UTS) being used at the time 
        /// production measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<PositioningTech>, PositioningTech> PositioningTechStateEvents;

        /// <summary>
        /// Records the IDs of the designs selected on a machine at the time production measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<int>, int> DesignNameIDStateEvents;

        /// <summary>
        /// Records the state of the automatic machine control on the machine at the time measurements were being made.
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode> MachineAutomaticsStateEvents;

        /// <summary>
        /// Records the state of the selected machine gear at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<MachineGear>, MachineGear> MachineGearStateEvents;

        /// <summary>
        /// Records the state of minimum elevation mapping on the machine at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<bool>, bool> MinElevMappingStateEvents;

        /// <summary>
        /// Records the state of GPSAccuracy and accompanying GPSTolerance on the machine at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance> GPSAccuracyAndToleranceStateEvents;

        /// <summary>
        /// Records the selected Layer ID on the machine at the time measurements were being made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort> LayerIDStateEvents;

        /// <summary>
        /// Records the selected design on the machine at the time the measurements were made
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<string>, string> DesignNameStateEvents;

        /// <summary>
        /// ICFlags control flags change events
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<byte>, byte> ICFlagsStateEvents;

        /// <summary>
        /// Records the target CCV value configured on the machine control system
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<short>, short> TargetCCVStateEvents;

        /// <summary>
        /// Records the target CCA value configured on the machine control system
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<short>, short> TargetCCAStateEvents;

        /// <summary>
        /// Records the target MDP value configured on the machine control system
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<short>, short> TargetMDPStateEvents;

        /// <summary>
        /// Records the target MDP value configured on the machine control system
        /// </summary>
        public ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort> TargetPassCountStateEvents;

        /// <summary>
        /// Create all defined event lists in one operation.
        /// </summary>
        private void CreateEventLists()
        {
            StartEndRecordedDataEvents = new StartEndRecordedDataChangeList(MachineID, SiteModel.ID, ProductionEventType.StartRecordedData);
            VibrationStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<VibrationState>, VibrationState>(MachineID, SiteModel.ID, ProductionEventType.VibrationStateChange);
            AutoVibrationStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<AutoVibrationState>, AutoVibrationState>(MachineID, SiteModel.ID, ProductionEventType.AutoVibrationStateChange);
            GPSModeStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<GPSMode>, GPSMode>(MachineID, SiteModel.ID, ProductionEventType.GPSModeChange);
            PositioningTechStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<PositioningTech>, PositioningTech>(MachineID, SiteModel.ID, ProductionEventType.PositioningTech);
            DesignNameIDStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<int>, int>(MachineID, SiteModel.ID, ProductionEventType.DesignChange);
            MachineAutomaticsStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode>(MachineID, SiteModel.ID, ProductionEventType.MachineAutomaticsChange);
            MachineGearStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<MachineGear>, MachineGear>(MachineID, SiteModel.ID, ProductionEventType.MachineGearChange);
            MinElevMappingStateEvents = new ProductionEventChangeList< ProductionEventChangeBase<bool>, bool>(MachineID, SiteModel.ID, ProductionEventType.MinElevMappingStateChange);
            GPSAccuracyAndToleranceStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance>(MachineID, SiteModel.ID, ProductionEventType.GPSAccuracyChange);
            LayerIDStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort>(MachineID, SiteModel.ID, ProductionEventType.LayerID);
            DesignNameStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<string>, string>(MachineID, SiteModel.ID, ProductionEventType.DesignChange);
            ICFlagsStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<byte>, byte>(MachineID, SiteModel.ID, ProductionEventType.ICFlagsChange);            
            TargetCCVStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<short>, short>(MachineID, SiteModel.ID, ProductionEventType.TargetCCV);
            TargetCCAStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<short>, short>(MachineID, SiteModel.ID, ProductionEventType.TargetCCA);
            TargetMDPStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<short>, short>(MachineID, SiteModel.ID, ProductionEventType.TargetMDP);
            TargetPassCountStateEvents = new ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort>(MachineID, SiteModel.ID, ProductionEventType.TargetPassCount);
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
                DesignNameStateEvents,
                ICFlagsStateEvents,
                TargetCCVStateEvents,
                TargetCCAStateEvents,
                TargetMDPStateEvents,
                TargetPassCountStateEvents
            };
        }
    /// <summary>
    /// Primary constructor for events recorded by a single machine within a single site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="machineID"></param>
    public ProductionEventChanges(SiteModel siteModel, long machineID)
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
            StartEndRecordedDataEvents = StartEndRecordedDataEvents.LoadFromStore(storageProxy) as StartEndRecordedDataChangeList;
            VibrationStateEvents = VibrationStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<VibrationState>, VibrationState>;
            AutoVibrationStateEvents = AutoVibrationStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<AutoVibrationState>, AutoVibrationState>;
            GPSModeStateEvents = GPSModeStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<GPSMode>, GPSMode>;
            PositioningTechStateEvents = PositioningTechStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<PositioningTech>, PositioningTech>;
            DesignNameIDStateEvents = DesignNameIDStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<int>, int>;
            MachineAutomaticsStateEvents = MachineAutomaticsStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<MachineAutomaticsMode>, MachineAutomaticsMode>;
            MachineGearStateEvents = MachineGearStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<MachineGear>, MachineGear>;
            MinElevMappingStateEvents = MinElevMappingStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<bool>, bool>;
            GPSAccuracyAndToleranceStateEvents = GPSAccuracyAndToleranceStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<GPSAccuracyAndTolerance>, GPSAccuracyAndTolerance>;
            LayerIDStateEvents = LayerIDStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort>;
            DesignNameStateEvents = DesignNameStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<string>, string>;
            ICFlagsStateEvents = DesignNameStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<byte>, byte>;
            TargetCCVStateEvents = TargetCCVStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<short>, short>;
            TargetCCAStateEvents = TargetCCAStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<short>, short>;
            TargetMDPStateEvents = TargetMDPStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<short>, short>;
            TargetPassCountStateEvents = TargetPassCountStateEvents.LoadFromStore(storageProxy) as ProductionEventChangeList<ProductionEventChangeBase<ushort>, ushort>;

            return true;
        }
    }
}
