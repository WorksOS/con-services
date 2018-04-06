namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// Elevation type controls the cell pass from which to select an elevation when a set of cell passes has been 
    /// selected by a filter.
    /// </summary>
    public enum ElevationType
    {
        /// <summary>
        /// Elevation of the last measured call pass (latest date)
        /// </summary>
        Last,

        /// <summary>
        /// Elevation of the first measured call pass (earliest date)
        /// </summary>
        First,

        /// <summary>
        /// Elevation of the highest measured cell pass
        /// </summary>
        Highest,

        /// <summary>
        /// Elevation of the lowest measured cell pass
        /// </summary>
        Lowest
    }
}
