namespace VSS.TRex.TAGFiles.Types
{
    /// <summary>
    /// The mode to use when integrating one subgrid tree into another during TAG file processing
    /// </summary>
    public enum SubGridTreeIntegrationMode
    {
        /// <summary>
        /// Subgrid trees are being integrated into an intermediary subgrid tree whose lifecycle is restricted to in-memory
        /// </summary>
        UsingInMemoryTarget,

        /// <summary>
        /// Subgrid trees are being integrated into the master subgrid tree held in persistent storage
        /// </summary>
        SaveToPersistentStore
    }
}
