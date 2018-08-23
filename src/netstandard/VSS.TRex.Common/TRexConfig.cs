using System.IO;

namespace VSS.TRex.Common
{
  /// <summary>
  /// A class to contain a collection of TRex configuration controls.
  /// Should be refactored or modified so the standard c# configuration system is used or underpins it.
  ///
  /// Important! configuration will come from a json file or environment variables so dont use this class. For long term see program.cs for ConfigurationBuilder 
  ///
  /// 
  /// </summary>
  public static class TRexConfig
    {
        /// <summary>
        /// The limit under which node subgrids are represented by sparse lists rather than a complete subgrid array of child subgrid references
        /// </summary>
        /// <returns></returns>
        public static int SubGridTreeNodeCellSparcityLimit() => 20;

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

        /// <summary>
        /// The file system location in which to store Ignite persistent data
        /// </summary>
        public static string PersistentCacheStoreLocation = Path.Combine(Path.GetTempPath(), "TRexIgniteData");
        //public static string PersistentCacheStoreLocation = "C:/temp/TRexIgniteData"; //Path.Combine(Path.GetTempPath(), "TRexIgniteData");
    
      /*
        /// <summary>
        /// Use TFA service to validate tagfiles
        /// </summary>
        public static bool EnableTFAService = true;

        /// <summary>
        /// Enable archiving of tagfiles
        /// </summary>
        public static bool EnableTagfileArchiving = false; // for now

        /// <summary>
        /// Enable archiving of the metadata of tagfiles
        /// </summary>
        public static bool EnableTagfileArchivingMetaData = false;

        /// <summary>
        /// URL of TFA
        /// </summary>
        public static string TFAServiceURL = "http://localhost:7654/";

        /// <summary>
        /// Endpoint for validation of tagfiles in TFA service
        /// </summary>
        public static string TFAServiceGetProjectID = "api/v3/project/getid";

        /// <summary>
        /// Minimum lenght a valid tagfile can be. e.g header is normally over 100 bytes
        /// </summary>
        public static int MinTAGFileLength = 100;

        /// <summary>
        /// location to archive tagfiles. If blank defaults to local temp TrexIgniteData folder
        /// </summary>
        public static string TagFileArchiveFolder = "";
        */
    }
}
