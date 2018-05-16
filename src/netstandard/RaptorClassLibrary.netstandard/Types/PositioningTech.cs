namespace VSS.TRex.Types
{
    /// <summary>
    /// The technology used to provide the reported positions used to process cell passes
    /// </summary>
    public enum PositioningTech
    {
        /// <summary>
        /// Positions are derived using Global Positioning Systems
        /// </summary>
        GPS,

        /// <summary>
        /// Positions are derived usign a Universal Total Station
        /// </summary>
        UTS,

        /// <summary>
        /// Positioning tehnology used is unknown
        /// </summary>
        Unknown
    }
}
