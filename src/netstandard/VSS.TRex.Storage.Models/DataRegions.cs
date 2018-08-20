namespace VSS.TRex.Storage.Models
{
    /// <summary>
    /// Handles actions and configuration related to data regions introduced in Ignite 2.3 to allow per-cache control of persistency, memory usage etc
    /// </summary>
    public static class DataRegions
    {
        /// <summary>
        /// Default size of pages in the persitent storage, 16 Kb at the current time
        /// </summary>
        public const int DEFAULT_DATA_REGION_PAGE_SIZE = 16 * 1024;

        /// <summary>
        /// Default size of pages in the persitent storage
        /// </summary>
        public const int DEFAULT_MUTABLE_DATA_REGION_PAGE_SIZE = DEFAULT_DATA_REGION_PAGE_SIZE;

        /// <summary>
        /// Default size of pages in the persitent storage
        /// </summary>
        public const int DEFAULT_IMMUTABLE_DATA_REGION_PAGE_SIZE = DEFAULT_DATA_REGION_PAGE_SIZE;
        
        /// <summary>
        /// The name of the default data region (an undifferentiated data region in the grid)
        /// </summary>
        public const string DEFAULT_DATA_REGION_NAME = "Default";

        /// <summary>
        /// The name of the default mutable data region in the grid
        /// </summary>
        public const string DEFAULT_MUTABLE_DATA_REGION_NAME = "Default-Mutable";

        /// <summary>
        /// The name of the default mutable data region in the grid
        /// </summary>
        public const string DEFAULT_IMMUTABLE_DATA_REGION_NAME = "Default-Immutable";

        /// <summary>
        /// The data region to place mutable spatial data cache information into
        /// </summary>
        public const string MUTABLE_SPATIAL_DATA_REGION = DEFAULT_MUTABLE_DATA_REGION_NAME;

        /// <summary>
        /// The data region to place immutable spatial data cache information into
        /// </summary>
        public const string IMMUTABLE_SPATIAL_DATA_REGION = DEFAULT_IMMUTABLE_DATA_REGION_NAME;

        /// <summary>
        /// The data region to place mutable nonspatial data cache information into
        /// </summary>
        public const string MUTABLE_NONSPATIAL_DATA_REGION = DEFAULT_MUTABLE_DATA_REGION_NAME;

        /// <summary>
        /// The data region to place immutable nonspatial data cache information into
        /// </summary>
        public const string IMMUTABLE_NONSPATIAL_DATA_REGION = DEFAULT_IMMUTABLE_DATA_REGION_NAME;

        /// <summary>
        /// The data region to place spatial subgrid existance maps (usually computed from TTM topological designs and surveyed surfaces)
        /// </summary>
        public const string SPATIAL_EXISTENCEMAP_DATA_REGION = DEFAULT_IMMUTABLE_DATA_REGION_NAME;
        /// <summary>
        /// The data region to place spatial subgrid existance maps (usually computed from TTM topological designs and surveyed surfaces)
        /// </summary>
        public const string TAG_FILE_BUFFER_QUEUE_DATA_REGION = "TAGFileBufferQueue";

    }
}
