using System;

namespace VSS.TRex.Common
{
  public static class Consts
  {
    public const double TOLERANCE_PERCENTAGE = 0.00001;
    public const double TOLERANCE_DIMENSION = 0.00001;
    public const double TOLERANCE_TEMPERATURE = 0.00001;
    public const double TOLERANCE_HEIGHT = 0.00001;
    public const double TOLERANCE_AREA = 0.00001;
    public const double TOLERANCE_DECIMAL_DEGREE = 1e-10;
    public const double MIN_ELEVATION = -1e10;
    public const double MAX_ELEVATION = 1e10;

    public const double NullReal = 1E308;

    public static readonly DateTime MIN_DATETIME_AS_UTC = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
    public static readonly DateTime MAX_DATETIME_AS_UTC = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

    /// <summary>
    /// IEEE single/float null value
    /// </summary>
    public const float NullSingle = float.MaxValue;

    /// <summary>
    /// IEEE single/float null value
    /// </summary>
    public const float NullFloat = float.MaxValue;

    /// <summary>
    /// IEEE double null value
    /// </summary>
    public const double NullDouble = NullReal;

    /// <summary>
    /// Value representing a null height encoded as an IEEE single
    /// </summary>
    public const float NullHeight = -3.4E38f;

    public const int NullLowestPassIdx = -1;

    public const int NumberCellPassesToBeFetched= 1000;

    // Value representing a null machine speed encoded as an IEEE ushort
    public const ushort NullMachineSpeed = ushort.MaxValue;

    /// <summary>
    /// Null ID for a design reference descriptor ID
    /// </summary>
    public const string kNoDesignName = "'<No Design>";
    public const int kNoDesignNameID = 0;

    /// <summary>
    /// ID representing any design ID in a filter
    /// </summary>
    public const int kAllDesignsNameID = -1;

    /// <summary>
    /// Largest GPS accuracy error value
    /// </summary>
    public const ushort kMaxGPSAccuracyErrorLimit = 0x3FFF;

    /// <summary>
    /// ID a placeholder for LegacyAssetId as not available in TRex
    /// </summary>
    /// <returns></returns>
    public const long NULL_LEGACY_ASSETID = -1;


    /// <summary>
    /// The limit under which node subgrids are represented by sparse lists rather than a complete sub grid array of child sub grid references
    /// </summary>
    /// <returns></returns>
    public const int SUBGRIDTREENODE_CELLSPARCITYLIMIT = 10;        // override with:  SUBGRIDTREENODE_CELLSPARCITYLIMIT

    /// <summary>
    /// The number of passes to increment a cell pass array by, when constructing filtered cell pass arrays
    /// </summary>
    public const int VLPDPSNODE_CELL_PASS_AGGREGATOR_LIST_SIZE_INCREMENT_DEFAULT = 100; // override with:  VLPDPSNODE_CELL_PASS_AGGREGATOR_LIST_SIZE_INCREMENT_DEFAULT

    /// <summary>
    /// Defines the maximum number of cell passes permitted in a sub grid segment before that segment will be split
    /// </summary>
    public const int VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT = 10000;     // override with:  VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT

    /// <summary>
    /// Defines the maximum number of cell passes that may occur within a single cell within a segment
    /// </summary>
    public const int VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT = 250;   // override with: VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT
    
    /// <summary>
    /// Record the result of each segment cleave operation to the log
    /// </summary>
    public const bool SEGMENTCLEAVINGOOPERATIONS_TOLOG = false;       // override with: SEGMENTCLEAVINGOOPERATIONS_TOLOG

    /// <summary>
    /// Records meta data about items to the log as they are written into the persistent store
    /// </summary>
    public const bool ITEMSPERSISTEDVIADATAPERSISTOR_TOLOG = false;  // override with: ITEMSPERSISTEDVIADATAPERSISTOR_TOLOG
    
    /// <summary>
    /// Enforces integrity on segments when they are added
    /// </summary>
    public const bool DEBUG_PERFORMSEGMENT_ADDITIONALINTEGRITYCHECKS = false;       // override with: DEBUG_PERFORMSEGMENT_ADDITIONALINTEGRITYCHECKS

    /// <summary>
    /// Paints a red diagonal cross on each rendered tile to aid in confirming correct registration of rendered data
    /// </summary>
    public const bool DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES = false;      // override with: DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES

    /// <summary>
    /// Controls notification of site model changes made in the persistent to interested listeners
    /// such as cluster processing nodes in the immutable grid
    /// </summary>
    public const bool ADVISEOTHERSERVICES_OFMODELCHANGES = true;       // override with: ADVISEOTHERSERVICES_OFMODELCHANGES
    
    /// <summary>
    /// Maximum number of TAG files to processing through the aggregation/integration pipeline
    /// as a single work unit
    /// </summary>
    public const int MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH = 20;  // override with: MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH

    /// <summary>
    /// Maximum number of TAG files the TAG file grouper will assemble into a single package of TAG files for swather processing
    /// </summary>
    public const int MAX_GROUPED_TAG_FILES_TO_PROCESS_PER_PROCESSING_EPOCH = 100; // override with: MAX_GROUPED_TAG_FILES_TO_PROCESS_PER_PROCESSING_EPOCH

    /// <summary>
    /// The number of partitions configured for caches that store spatial subgrid data
    /// </summary>
    public const int NUMPARTITIONS_PERDATACACHE = 1024;                   // override with: NUMPARTITIONS_PERDATACACHE

    /// <summary>
    /// The minimum tag file size required to contain even basic configuration
    /// </summary>
    public const int kMinTagFileLengthDefault = 100;                              // override with:  MIN_TAGFILE_LENGTH

    /// <summary>
    /// The minimum tag file size required to contain even basic configuration
    /// </summary>
    public const bool ENABLE_TFA_SERVICE = true;                       // override with:  ENABLE_TFA_SERVICE

    /// <summary>
    /// Archive tag file (this has normally already been done prior to submission to TRex 
    /// </summary>
    public const bool ENABLE_TAGFILE_ARCHIVING = false;                   // override with: ENABLE_TAGFILE_ARCHIVING

    /// <summary>
    /// Archive metadata with tag file
    /// </summary>
    public const bool ENABLE_TAGFILE_ARCHIVING_METADATA = false;            // override with: ENABLE_TAGFILE_ARCHIVING_METADATA

    /// <summary>
    /// Cache intermediary subgrid results for reuse in subsequent requests
    /// </summary>
    public const bool ENABLE_GENERAL_SUBGRID_RESULT_CACHING = true;                 // override with: ENABLE_GENERAL_SUBGRID_RESULT_CACHING

    /// <summary>
    /// The time interval between heart beat logging epochs, in milliseconds. Default is 10 seconds
    /// </summary>
    public const int HEARTBEAT_LOGGER_INTERVAL = 10000;              // override with: HEARTBEAT_LOGGER_INTERVAL

    /// <summary>
    /// The maximum number of subgrid elements to store in the general subgrid result cache
    /// </summary>
    public const int GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_ELEMENT_COUNT = 1_000_000; // override with GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_ELEMENT_COUNT

    /// <summary>
    /// The maximum aggregate size in bytes of all data stores in the general subgrid result cache
    /// </summary>
    public const long GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_SIZE = 1_000_000_000; // override with GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_SIZE

    /// <summary>
    /// The fraction of the Most Recently Used elements stored in the cache that are not such to
    /// moving to the MRU location in the cache as a result of being touched via a Get() operation
    /// </summary>
    public const double GENERAL_SUBGRID_RESULT_CACHE_DEAD_BAND_FRACTION = 0.33;   // override with GENERAL_SUBGRID_RESULT_CACHE_DEAD_BAND_FRACTION

    /// <summary>
    /// The time period the spatial memory cache waits between performing maintenance checks on the content of the cache
    /// </summary>
    public const int SPATIAL_MEMORY_CACHE_INTER_EPOCH_SLEEP_TIME_SECONDS = 600;   // override with SPATIAL_MEMORY_CACHE_INTER_EPOCH_SLEEP_TIME_SECONDS

    /// <summary>
    /// The time period the spatial memory cache will wait between a cache context being marked as invalidated and it being
    /// proactively removed from the cache. This is to allow concurrent operations with references to the invalidated cache
    /// to either expire or to re-validate the cache.
    /// </summary>
    public const int SPATIAL_MEMORY_CACHE_INVALIDATED_CACHE_CONTEXT_REMOVAL_WAIT_TIME_SECONDS = 600; // override with SPATIAL_MEMORY_CACHE_INVALIDATED_CACHE_CONTEXT_REMOVAL_WAIT_TIME_SECONDS

    /// <summary>
    /// The maximum number of passCount or Veta records to be exported
    /// </summary>
    public const int DEFAULT_MAX_EXPORT_ROWS = 10000000; // override with MAX_EXPORT_RECORDS 

    /// <summary>
    /// The number of concurrent tasks processing TAG files into site models in the mutable server nodes
    /// </summary>
    public const int NUM_CONCURRENT_TAG_FILE_PROCESSING_TASKS = 2; // override with NUM_CONCURRENT_TAG_FILE_PROCESSING_TASKS

    /// <summary>
    /// Rhe default capacity for memory streams created in a transient context, such as serialization
    /// </summary>
    public const int TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION = 32 * 1024; // override with TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION
  }
}
