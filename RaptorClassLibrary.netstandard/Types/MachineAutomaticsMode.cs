namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// The mode of the automatics blade control system within the machine control software
    /// </summary>
    public enum MachineAutomaticsMode
    {
        /// <summary>
        /// Automatics mode is unavailable or unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Blade control is manually operated
        /// </summary>
        Manual,

        /// <summary>
        /// Blade control is automatically operated by the machien control system
        /// </summary>
        Automatics
    }

    /// <summary>
    /// Machine control state flags. The flags relate to the bits set in the machine control
    /// state flags sent in production TAG files.
    /// </summary>
    public static class MachineControlStateFlags
    {
        /// <summary>
        /// Manual controls are enabled
        /// </summary>
        public const int GCSControlStateManual = 0x1;

        /// <summary>
        /// Active automatic controls are beign used
        /// </summary>
        public const int GCSControlStateInActiveAuto = 0x2;

        /// <summary>
        /// Automatics ar active, but the machibe is not moving
        /// </summary>
        public const int GCSControlStateAutoValueNotDriving = 0x4;

        /// <summary>
        /// Automatics controls are enabled
        /// </summary>
        public const int GCSControlStateAuto = 0x8;

        /// <summary>
        /// Null value for machien automatics control state
        /// </summary>
        public const int NullGCSControlState = 0;
    }

}
