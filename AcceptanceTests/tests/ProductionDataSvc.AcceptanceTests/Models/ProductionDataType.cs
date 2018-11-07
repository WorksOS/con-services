namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// Underlying data types requestable from the Raptor database in the context of profiles. 
    /// Not all data themes are support in all contexts.
    /// </summary>
    /// 
    public enum ProductionDataType
    {
        /// <summary>
        /// All attributes are returned. Typically not supported.
        /// </summary>
        /// 
        All = 0,

        /// <summary>
        /// CCV compaction measurements
        /// </summary>
        /// 
        CCV = 1,

        /// <summary>
        /// Elevation measurements
        /// </summary>
        /// 
        Height = 2,

        /// <summary>
        /// Radio latency measurements
        /// </summary>
        /// 
        Latency = 3,

        /// <summary>
        /// Pass counts derived from layer analysis
        /// </summary>
        /// 
        PassCount = 4,

        /// <summary>
        /// Vibratory drum frequency
        /// </summary>
        /// 
        Frequency = 5,

        /// <summary>
        /// Vibratory drum amplitude
        /// </summary>
        /// 
        Amplitude = 6,

        /// <summary>
        /// Soil moisture level
        /// </summary>
        /// 
        Moisture = 7,

        /// <summary>
        /// Asphalt mat temperature
        /// </summary>
        /// 
        Temperature = 8,

        /// <summary>
        /// Resonance meter value from vibratory compaction system
        /// </summary>
        /// 
        RMV = 9,

        /// <summary>
        /// GPS guidance mode
        /// </summary>
        /// 
        GPSMode = 10,

        /// <summary>
        /// CCV measurement as a percentage of the target value
        /// </summary>
        /// 
        CCVPercent = 11,

        /// <summary>
        /// Spatial indication of volume computation locality
        /// </summary>
        /// 
        SimpleVolumeOverlay = 12,

        /// <summary>
        /// Combined height and time information
        /// </summary>
        /// 
        HeightAndTime = 13,

        /// <summary>
        /// Elevations combining production and surveyed surface data
        /// </summary>
        /// 
        CompositeHeights = 14,

        /// <summary>
        /// MDP compaction sensor values
        /// </summary>
        /// 
        MDP = 15,

        /// <summary>
        /// MDP measurements as a percentage of the target value
        /// </summary>
        /// 
        MDPPercent = 16,

        /// <summary>
        /// Layer analysis and breakdown
        /// </summary>
        /// 
        CellProfile = 17,

        /// <summary>
        /// Layer analysis and breakdown including passes
        /// </summary>
        /// 
        CellPasses = 18,

        /// <summary>
        /// Machine speed
        /// </summary>
        /// 
        MachineSpeed = 19,

        /// <summary>
        /// Returns previous CCV value over the most recent
        /// </summary>
        CCVChange = 20
    }
}
