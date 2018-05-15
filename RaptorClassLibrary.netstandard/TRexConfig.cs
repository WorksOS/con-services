namespace VSS.TRex
{
    /// <summary>
    /// A class to contain a collection of TRex configuration controls.
    /// Shoulb be refactored or modified so the standard c# configuration system is used or underpins it.
    /// </summary>
    public static class TRexConfig
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
        /// Defines the maximum number of cell passes permitted in a subgrid segment before that segment will be split
        /// </summary>
        public static int VLPD_SubGridSegmentPassCountLimit = 15000;

        /// <summary>
        /// Defines the maximum number of cell passes that may occur within a single cell within a segment
        /// </summary>
        public static int VLPD_SubGridMaxSegmentCellPassesLimit = 250;

        /// <summary>
        /// The number of paritions configured for caches that store spatial subgrid data
        /// </summary>
        public static uint NumPartitionsPerDataCache = 1024;


        public static bool EnableTFAService = true;
        public static string TFAServiceURL = "http://localhost:7654/";
        public static string TFAServiceGetProjectID = "api/v3/project/getid";
        public static int MinTAGFileLength = 100;
    }
}
