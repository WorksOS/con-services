using System;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Machines
{
    /// <summary>
    /// Defines all the metadata relating to a machine that has contributed in some way to a site model. These machine instances
    /// are relevant to individual sitemodels. There will be a machine instance within each site model that the machine has
    /// contributed to.
    /// </summary>
    [Serializable]
    public class Machine
    {
        /// <summary>
        /// The telematics asset ID assigned to any John Doe machine
        /// </summary>
        public const long kJohnDoeAssetID = 0x7fffffffffffffff;

        [NonSerialized]
        public MachinesList Owner;

        public Guid ID { get; set; }

        public short InternalSiteModelMachineIndex { get; set; }

        public string Name { get; set; } = "";

        public byte MachineType { get; set; } = byte.MaxValue;

        public int DeviceType { get; set; } = int.MaxValue;

        public string MachineHardwareID { get; set; } = "";

        public bool IsJohnDoeMachine { get; set; }

        public double LastKnownX { get; set; } = Consts.NullDouble;
        public double LastKnownY { get; set; } = Consts.NullDouble;
        public DateTime LastKnownPositionTimeStamp { get; set; } = DateTime.MinValue;

        public string LastKnownDesignName { get; set; } = string.Empty;

        public ushort LastKnownLayerId { get; set; }

        private bool _compactionDataReported;

        /// <summary>
        /// Indicates if the machine has ever reported any compactrion realated data, such as CCV, MDP or CCA measurements
        /// </summary>
        public bool CompactionDataReported { get => _compactionDataReported; set => _compactionDataReported = _compactionDataReported | value; }

        public CompactionSensorType CompactionSensorType { get; set; } = CompactionSensorType.NoSensor;
    
        [NonSerialized]
        public ProductionEventLists TargetValueChanges = null; // new ProductionEventChanges(null, -1);

        /// <summary>
        /// Determines if the type of this machine is one of the machine tyeps that supports compaction operations
        /// </summary>
        /// <returns></returns>
        public bool MachineIsConpactorType()
        {
            return MachineType == (byte)Types.MachineType.SoilCompactor ||
                   MachineType == (byte)Types.MachineType.AsphaltCompactor ||
                   MachineType == (byte)Types.MachineType.FourDrumLandfillCompactor;
        }

        public static bool MachineGearIsForwardGear(MachineGear gear)
        {
            return gear == MachineGear.Forward || gear == MachineGear.Forward2 || gear == MachineGear.Forward3 || gear == MachineGear.Forward4 || gear == MachineGear.Forward5;
        }

        public static bool MachineGearIsReverseGear(MachineGear gear)
        {
            return gear == MachineGear.Reverse || gear == MachineGear.Reverse2 || gear == MachineGear.Reverse3 || gear == MachineGear.Reverse4 || gear == MachineGear.Reverse5;
        }

        public Machine()
        {
        }

        public Machine(MachinesList owner) : this()
        {
            Owner = owner;
        }

        public Machine(MachinesList owner,
                       string name,
                       string machineHardwareID,
                       byte machineType,
                       int deviceType,
                       Guid machineID,
                       short internalSiteModelMachineIndex,
                       bool isJohnDoeMachine
                       /* TODO: AConnectedMachineLevel : MachineLevelEnum*/) : this(owner)
        {
            Name = name;
            MachineHardwareID = machineHardwareID;
            MachineType = machineType;
            DeviceType = deviceType;

            IsJohnDoeMachine = isJohnDoeMachine;
            ID = machineID;
            InternalSiteModelMachineIndex = internalSiteModelMachineIndex;

            // TODO FConnectedMachineLevel:= AConnectedMachineLevel;
        }

        public void Assign(Machine source)
        {
            Name = source.Name;
            MachineHardwareID = source.MachineHardwareID;
            CompactionSensorType = source.CompactionSensorType;
            //            CompactionRMVJumpThreshold = source.CompactionRMVJumpThreshold;
            //            UseMachineRMVThreshold = source.UseMachineRMVThreshold;
            //            OverrideRMVJumpThreshold = source.OverrideRMVJumpThreshold;
            DeviceType = source.DeviceType;
            CompactionDataReported = source.CompactionDataReported;
            //            ConnectedMachineLevel = source.ConnectedMachineLevel;
            MachineType = source.MachineType;
            IsJohnDoeMachine = source.IsJohnDoeMachine;
            LastKnownX = source.LastKnownX;
            LastKnownY = source.LastKnownY;
            LastKnownLayerId = source.LastKnownLayerId;
            LastKnownDesignName = source.LastKnownDesignName;
            LastKnownPositionTimeStamp = source.LastKnownPositionTimeStamp;

            //            Dirty = True;
        }
    }
}
