namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// The state of machine control automatic drum vibration
    /// </summary>
    public enum AutoVibrationState
    {
        /// <summary>l
        /// Automatics are off
        /// </summary>
        Off = 0,

        /// <summary>
        /// Automatics are under control of the machine control system
        /// </summary>
        Auto = 1,

        /// <summary>
        /// Vibration is manually operated by the user
        /// </summary>
        Manual = 2,

        /// <summary>
        /// Unknown/invalid vibration state
        /// </summary>
        Unknown = 3
    }
}
