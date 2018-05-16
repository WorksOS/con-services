namespace VSS.TRex.TAGFiles.Types
{
    /// <summary>
    /// EpochStateEvent defines a list of events that may occur during an
    /// epoch (ie: within the time interval between successive time stamps).
    /// These events are considered to happen at the time of the last read
    /// time stamp, and are not considered to be a part of the epoch processing
    /// that occurs at every time stamp after the initial time stamp in the
    /// tag file.
    /// </summary>
    public enum EpochStateEvent
    {
        /// <summary>
        /// Null event
        /// </summary>
        Unknown,

        /// <summary>
        /// The machine has been started up
        /// </summary>
        MachineStartup,

        /// <summary>
        /// The machine has been shutdown.
        /// </summary>
        MachineShutdown,

        /// <summary>
        /// Ther machine has reset the accumulation of pass counts on the machine
        /// </summary>
        MachineMapReset,  

        /// <summary>
        /// The machine is operating in Universal Total Station mode
        /// </summary>
        MachineInUTSMode
    }
}
