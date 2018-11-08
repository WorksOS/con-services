namespace VSS.TRex.Types
{
    /// <summary>
    /// Type of GPS position used for location information available for a cell pass
    /// </summary>
    public enum GPSMode
    {
        /// <summary>
        /// Old, unrealiable, GPS mode
        /// </summary>
        Old = 0,

        /// <summary>
        /// Automonous (low accuracy) GPS mode
        /// </summary>
        AutonomousPosition = 1,

        /// <summary>
        /// Float RTK GPS mode
        /// </summary>
        Float = 2,

        /// <summary>
        /// Float RTK GPS mode
        /// </summary>
        Fixed = 3,

        /// <summary>
        /// Differential GPS mode
        /// </summary>
        DGPS = 4,

        /// <summary>
        /// Unused, placeholder
        /// </summary>
        Unknown5 = 5,

        /// <summary>
        /// Unused, placeholder
        /// </summary>
        Unknown6 = 6,

        /// <summary>
        /// SBAS (Satellite-based augmentation system) GPS mode
        /// </summary>
        SBAS = 7,

        /// <summary>
        /// 'Location' RTK GPS mode
        /// </summary>
        LocationRTK = 8,

        /// <summary>
        /// No GPS location available
        /// </summary>
        NoGPS = 0x0F
    }
}
