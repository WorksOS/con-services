namespace VSS.TRex.Types
{
    /// <summary>
    /// Controls whether any layer analysis is applied to cell passes in cells queried from the data model
    /// </summary>
    public enum LayerState
    {
        /// <summary>
        /// No layer analysis is performance
        /// </summary>
        Off,

        /// <summary>
        /// Layer analysis is performed according to LayerMethod
        /// </summary>
        On,

        /// <summary>
        /// Null value for this enum
        /// </summary>
        Invalid
    }
}
