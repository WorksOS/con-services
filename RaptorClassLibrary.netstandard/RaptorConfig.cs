namespace VSS.VisionLink.Raptor
{
    /// <summary>
    /// A class to contain a collection of Raptor configuration controls.
    /// Shoulb be refactored or modified so the standard c# configuration system is used or underpins it.
    /// </summary>
    public static class RaptorConfig
    {
        /// <summary>
        /// The limit under which node subgrids are represented by sparse lists rather than a complete subgrid array of child subgrid references
        /// </summary>
        /// <returns></returns>
        public static int SubGridTreeNodeCellSparcityLimit() => 20;

        /// <summary>
        /// The default first asset ID number used for a John Doe machine
        /// </summary>
        /// <returns></returns>
        public static long JohnDoeBaseNumber() => 1000000;

        /// <summary>
        /// The number of passes to increment a cell pass array by when constructing filtered cell pass arrays
        /// </summary>
        public static int VLPDPSNode_CellPassAggregationListSizeIncrement => 100;

        /// <summary>
        /// The number of spatial processing divisions within the Raptor cluster (immutable/read grid)
        /// </summary>
        public static uint numSpatialProcessingDivisions = 1;

        /// <summary>
        /// The number of nodes processing and storing data from TAG files (mutable grid)
        /// </summary>
        public static uint numTAGFileProcessingDivisions = 1;
    }
}
