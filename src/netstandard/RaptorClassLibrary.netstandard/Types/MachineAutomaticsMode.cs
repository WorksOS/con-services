namespace VSS.TRex.Types
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
}
