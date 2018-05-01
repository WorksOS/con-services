using System.Linq;
using System.Reflection;
using log4net;
using VSS.TRex.Events.Interfaces;
using VSS.VisionLink.Raptor.Events.Interfaces;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// A wrapper for all the event information related to a particular machine's activities within a particular
    /// site model.co
    /// </summary>
    public class ProductionEventLists : IProductionEventLists

    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The SiteModel these events relate to
        /// </summary>
        private SiteModel SiteModel { get; set; }

        /// <summary>
        /// The ID of the machine these events were recorded by
        /// </summary>
        public long MachineID { get; set; }

        public StartEndProductionEvents MachineStartupShutdownEvents;

        /// <summary>
        /// Events recording the Start and Stop events for recording production data on a machine
        /// </summary>
        public StartEndProductionEvents StartEndRecordedDataEvents;

        /// <summary>
        /// Events recording vibration state changes for vibratory drum compactor operation
        /// </summary>
        public ProductionEvents<VibrationState> VibrationStateEvents;

        /// <summary>
        /// Events recording automatics vibration state changes for vibratory drum compactor operation
        /// </summary>
        public ProductionEvents<AutoVibrationState> AutoVibrationStateEvents;

        /// <summary>
        /// Events recording changes to the prevailing GPSMode (eg: RTK Fixed, RTK Float, Differential etc) at the time 
        /// production measurements were being made
        /// </summary>
        public ProductionEvents<GPSMode> GPSModeStateEvents;

        /// <summary>
        /// Records the positioning technology (eg: GPS or UTS) being used at the time production measurements were being made
        /// </summary>
        public ProductionEvents<PositioningTech> PositioningTechStateEvents;

        /// <summary>
        /// Records the IDs of the designs selected on a machine at the time production measurements were being made
        /// </summary>
        public ProductionEvents<int> DesignNameIDStateEvents;

        /// <summary>
        /// Records the state of the automatic machine control on the machine at the time measurements were being made.
        /// </summary>
        public ProductionEvents<MachineAutomaticsMode> MachineAutomaticsStateEvents;

        /// <summary>
        /// Records the state of the selected machine gear at the time measurements were being made
        /// </summary>
        public ProductionEvents<MachineGear> MachineGearStateEvents;

        /// <summary>
        /// Records the state of minimum elevation mapping on the machine at the time measurements were being made
        /// </summary>
        public ProductionEvents<bool> MinElevMappingStateEvents;

        /// <summary>
        /// Records the state of GPSAccuracy and accompanying GPSTolerance on the machine at the time measurements were being made
        /// </summary>
        public ProductionEvents<GPSAccuracyAndTolerance> GPSAccuracyAndToleranceStateEvents;

        /// <summary>
        /// Records the selected Layer ID on the machine at the time measurements were being made
        /// </summary>
        public ProductionEvents<ushort> LayerIDStateEvents;

        /// <summary>
        /// Records the selected design on the machine at the time the measurements were made
        /// </summary>
//        public ProductionEvents<string> DesignNameStateEvents;

        /// <summary>
        /// ICFlags control flags change events
        /// </summary>
        public ProductionEvents<byte> ICFlagsStateEvents;

        /// <summary>
        /// Records the target CCV value configured on the machine control system
        /// </summary>
        public ProductionEvents<short> TargetCCVStateEvents;

        /// <summary>
        /// Records the target CCA value configured on the machine control system
        /// </summary>
        public ProductionEvents<short> TargetCCAStateEvents;

        /// <summary>
        /// Records the target MDP value configured on the machine control system
        /// </summary>
        public ProductionEvents<short> TargetMDPStateEvents;

        /// <summary>
        /// Records the target MDP value configured on the machine control system
        /// </summary>
        public ProductionEvents<ushort> TargetPassCountStateEvents;

        /// <summary>
        /// Records the target minimum temperature value configured on the machine control system
        /// </summary>
        public ProductionEvents<ushort> TargetMinMaterialTemperature;

        /// <summary>
        /// Records the target maximum temperature value configured on the machine control system
        /// </summary>
        public ProductionEvents<ushort> TargetMaxMaterialTemperature;

        /// <summary>
        /// Records the target lift thickness value configured on the machine control system
        /// </summary>
        public ProductionEvents<float> TargetLiftThickness;

        /// <summary>
        /// Records the Resonance Meter Value jump threshold configured on the machine control system
        /// </summary>
        public ProductionEvents<short> RMVJumpThresholdEvents;

        /// <summary>
        /// Create all defined event lists in one operation.
        /// </summary>
        private void CreateEventLists()
        {
            MachineStartupShutdownEvents = new StartEndProductionEvents(this, MachineID, SiteModel.ID,
                ProductionEventType.MachineStartupShutdown,
                (w, s) => w.Write((byte) s),
                r => (ProductionEventType) r.ReadByte());

            StartEndRecordedDataEvents = new StartEndProductionEvents(this, MachineID, SiteModel.ID,
                ProductionEventType.StartEndRecordedData,
                (w, s) => w.Write((byte) s),
                r => (ProductionEventType) r.ReadByte());

            VibrationStateEvents = new ProductionEvents<VibrationState>(this, MachineID, SiteModel.ID,
                ProductionEventType.VibrationStateChange,
                (w, s) => w.Write((byte) s),
                r => (VibrationState) r.ReadByte());

            AutoVibrationStateEvents = new ProductionEvents<AutoVibrationState>(this, MachineID, SiteModel.ID,
                ProductionEventType.AutoVibrationStateChange,
                (w, s) => w.Write((byte) s),
                r => (AutoVibrationState) r.ReadByte());

            GPSModeStateEvents = new ProductionEvents<GPSMode>(this, MachineID, SiteModel.ID,
                ProductionEventType.GPSModeChange,
                (w, s) => w.Write((byte) s),
                r => (GPSMode) r.ReadByte());

            PositioningTechStateEvents = new ProductionEvents<PositioningTech>(this, MachineID, SiteModel.ID,
                ProductionEventType.PositioningTech,
                (w, s) => w.Write((byte) s),
                r => (PositioningTech) r.ReadByte());

            DesignNameIDStateEvents = new ProductionEvents<int>(this, MachineID, SiteModel.ID,
                ProductionEventType.DesignChange,
                (w, s) => w.Write(s),
                r => r.ReadInt32());

            MachineAutomaticsStateEvents = new ProductionEvents<MachineAutomaticsMode>(this, MachineID, SiteModel.ID,
                ProductionEventType.MachineAutomaticsChange,
                (w, s) => w.Write((byte) s),
                r => (MachineAutomaticsMode) r.ReadByte());

            MachineGearStateEvents = new ProductionEvents<MachineGear>(this, MachineID, SiteModel.ID,
                ProductionEventType.MachineGearChange,
                (w, s) => w.Write((byte) s),
                r => (MachineGear) r.ReadByte());

            MinElevMappingStateEvents = new ProductionEvents<bool>(this, MachineID, SiteModel.ID,
                ProductionEventType.MinElevMappingStateChange,
                (w, s) => w.Write(s),
                r => r.ReadBoolean());

            GPSAccuracyAndToleranceStateEvents = new ProductionEvents<GPSAccuracyAndTolerance>(this, MachineID,
                SiteModel.ID, ProductionEventType.GPSAccuracyChange,
                (w, s) =>
                {
                    w.Write(s.GPSTolerance);
                    w.Write((byte) s.GPSAccuracy);
                },
                r => new GPSAccuracyAndTolerance((GPSAccuracy) r.ReadByte(), r.ReadUInt16()));

            LayerIDStateEvents = new ProductionEvents<ushort>(this, MachineID, SiteModel.ID,
                ProductionEventType.LayerID,
                (w, s) => w.Write(s), r => r.ReadUInt16());

            //            DesignNameStateEvents = new ProductionEvents<string>(this, MachineID, SiteModel.ID, ProductionEventType.DesignChange);

            ICFlagsStateEvents = new ProductionEvents<byte>(this, MachineID, SiteModel.ID,
                ProductionEventType.ICFlagsChange,
                (w, s) => w.Write(s), r => r.ReadByte());

            TargetCCVStateEvents = new ProductionEvents<short>(this, MachineID, SiteModel.ID,
                ProductionEventType.TargetCCV,
                (w, s) => w.Write(s), r => r.ReadInt16());

            TargetCCAStateEvents = new ProductionEvents<short>(this, MachineID, SiteModel.ID,
                ProductionEventType.TargetCCA,
                (w, s) => w.Write(s), r => r.ReadInt16());

            TargetMDPStateEvents = new ProductionEvents<short>(this, MachineID, SiteModel.ID,
                ProductionEventType.TargetMDP,
                (w, s) => w.Write(s), r => r.ReadInt16());

            TargetPassCountStateEvents = new ProductionEvents<ushort>(this, MachineID, SiteModel.ID,
                ProductionEventType.TargetPassCount,
                (w, s) => w.Write(s), r => r.ReadUInt16());

            TargetMinMaterialTemperature = new ProductionEvents<ushort>(this, MachineID, SiteModel.ID,
                ProductionEventType.TempWarningLevelMinChange,
                (w, s) => w.Write(s), r => r.ReadUInt16());

            TargetMaxMaterialTemperature = new ProductionEvents<ushort>(this, MachineID, SiteModel.ID,
                ProductionEventType.TempWarningLevelMaxChange,
                (w, s) => w.Write(s), r => r.ReadUInt16());

            TargetLiftThickness = new ProductionEvents<float>(this, MachineID, SiteModel.ID,
                ProductionEventType.TargetLiftThickness,
                (w, s) => w.Write(s), r => r.ReadSingle());

            RMVJumpThresholdEvents = new ProductionEvents<short>(this, MachineID, SiteModel.ID,
                ProductionEventType.MachineRMVJumpValueChange,
                (w, s) => w.Write(s), r => r.ReadInt16());
        }

        /// <summary>
        /// Returns an array containing all the event lists for a machine
        /// </summary>
        /// <returns></returns>
        public IProductionEvents[] GetEventLists()
        {
            return new IProductionEvents[]
            {
                MachineStartupShutdownEvents,
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
        public ProductionEventLists(SiteModel siteModel, long machineID)
        {
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
            foreach (IProductionEvents list in GetEventLists())
            {
                Log.Debug(
                    $"Saving {list.EventListType} with {list.Count()} events for machine {MachineID} in project {SiteModel.ID}");

                list.SaveToStore(storageProxy);
            }
        }

        public bool LoadEventsForMachine(IStorageProxy storageProxy)
        {
            MachineStartupShutdownEvents =
                MachineStartupShutdownEvents.LoadFromStore(storageProxy) as StartEndProductionEvents;
            StartEndRecordedDataEvents =
                StartEndRecordedDataEvents.LoadFromStore(storageProxy) as StartEndProductionEvents;
            VibrationStateEvents = VibrationStateEvents.LoadFromStore(storageProxy);
            AutoVibrationStateEvents = AutoVibrationStateEvents.LoadFromStore(storageProxy);
            GPSModeStateEvents = GPSModeStateEvents.LoadFromStore(storageProxy);
            PositioningTechStateEvents = PositioningTechStateEvents.LoadFromStore(storageProxy);
            DesignNameIDStateEvents = DesignNameIDStateEvents.LoadFromStore(storageProxy);
            MachineAutomaticsStateEvents = MachineAutomaticsStateEvents.LoadFromStore(storageProxy);
            MachineGearStateEvents = MachineGearStateEvents.LoadFromStore(storageProxy);
            MinElevMappingStateEvents = MinElevMappingStateEvents.LoadFromStore(storageProxy);
            GPSAccuracyAndToleranceStateEvents = GPSAccuracyAndToleranceStateEvents.LoadFromStore(storageProxy);
            LayerIDStateEvents = LayerIDStateEvents.LoadFromStore(storageProxy);

            //            DesignNameStateEvents = DesignNameStateEvents.LoadFromStore(storageProxy);

            ICFlagsStateEvents = ICFlagsStateEvents.LoadFromStore(storageProxy);

            TargetCCVStateEvents = TargetCCVStateEvents.LoadFromStore(storageProxy);
            TargetCCAStateEvents = TargetCCAStateEvents.LoadFromStore(storageProxy);
            TargetMDPStateEvents = TargetMDPStateEvents.LoadFromStore(storageProxy);
            TargetPassCountStateEvents = TargetPassCountStateEvents.LoadFromStore(storageProxy);
            TargetMinMaterialTemperature = TargetMinMaterialTemperature.LoadFromStore(storageProxy);
            TargetMaxMaterialTemperature = TargetMaxMaterialTemperature.LoadFromStore(storageProxy);
            TargetLiftThickness = TargetLiftThickness.LoadFromStore(storageProxy);

            RMVJumpThresholdEvents = RMVJumpThresholdEvents.LoadFromStore(storageProxy);

            // Wire the container (this object) into the list jsut read...
            GetEventLists().All(x =>
            {
                x.SetContainer(this);
                return true;
            });

            return true;
        }

        /// <summary>
        /// Provides a refernece to the TAG file processing start/end events list
        /// </summary>
        /// <returns></returns>
        public IProductionEventPairs GetStartEndRecordedDataEvents() => StartEndRecordedDataEvents;
    }
}
