using System;

namespace VSS.TRex.Events
{
    /// <summary>
    /// Denotes the types of events processed and stored from ingested production data
    /// </summary>
    [Serializable]
    public enum ProductionEventType
    {
        Unknown = 0x00000000,
        //MachineStartup = 0x00000001, Obsolete, now replaced by StartEvent
        //MachineShutdown = 0x00000002, Obsolete, now replaced by EndEvent
        //StartRecordedData = 0x00000003, Obsolete, now replaced by StartEvent
        //EndEndRecordedData = 0x00000004, Obsolete, now replaced by EndEvent
        StartEvent = 0x00000003,
        EndEvent = 0x00000004,
        DesignChange = 0x00000005,
        TargetCCV = 0x00000006,
        TargetPassCount = 0x00000007,
        MachineMapReset = 0x00000008,
        TargetLiftThickness = 0x00000009,
        GPSModeChange = 0x0000000A,
        VibrationStateChange = 0x0000000B,
        AutoVibrationStateChange = 0x0000000C,
        MachineGearChange = 0x0000000D,
        MachineAutomaticsChange = 0x0000000E,
        MachineRMVJumpValueChange = 0x0000000F,
        ICFlagsChange = 0x00000010,
        MinElevMappingStateChange = 0x00000011,
        GPSAccuracyChange = 0x00000012,
        PositioningTech = 0x00000013,
        InAvoidZone2DStateChange = 0x00000014,
        InAvoidZoneUSStateChange = 0x00000015,
        BladeOnGroundStateChange = 0x00000016,
        TempWarningLevelMinChange = 0x00000017,
        TempWarningLevelMaxChange = 0x00000018,
        TargetMDP = 0x00000019,
        LayerID = 0x0000001A,
        Sonic3DChange = 0x0000001B,
        DesignOverride = 0x0000001C,
        LayerOverride = 0x0000001D,
        TargetCCA = 0x0000001E,

        /// <summary>
        /// Used by an event list that has state events of ProductionEventType.StartEvent and ProductionEventType.EndEvent 
        /// to define periods of known recorded data (covered by processed TAG file data)
        /// </summary>
        StartEndRecordedData = 0x0000001F,

        /// <summary>
        /// Used by an event list that has state events of ProductionEventType.StartEvent and ProductionEventType.EndEvent 
        /// to define periods of known machine activity (startup -> shutdown periods)
        /// </summary>
        MachineStartupShutdown = 0x00000020
    }
}
