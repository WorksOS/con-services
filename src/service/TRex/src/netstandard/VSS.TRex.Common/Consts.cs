using System;

namespace VSS.TRex.Common
{
  public static class Consts
  {
    public const double TOLERANCE_PERCENTAGE = 0.00001;
    public const double TOLERANCE_DIMENSION = 0.00001;
    public const double TOLERANCE_TEMPERATURE = 0.00001;

    public const double NullReal = 1E308;

    /// <summary>
    /// IEEE single/float null value
    /// </summary>
    public const float NullSingle = Single.MaxValue;

    /// <summary>
    /// IEEE single/float null value
    /// </summary>
    public const float NullFloat = Single.MaxValue;

    /// <summary>
    /// IEEE double null value
    /// </summary>
    public const double NullDouble = NullReal; //Double.MaxValue;

    /// <summary>
    /// Value representing a null height encoded as an IEEE single
    /// </summary>
    public const float NullHeight = -3.4E38f;

    // Value representing a null machine speed encoded as an IEEE ushort
    public const ushort NullMachineSpeed = UInt16.MaxValue;

    /// <summary>
    /// Null ID for a design reference descriptor ID
    /// </summary>
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
    /// The limit under which node subgrids are represented by sparse lists rather than a complete subgrid array of child subgrid references
    /// </summary>
    /// <returns></returns>
    public const int kSubGridTreeNodeCellSparcityLimitDefault = 20;        // overide with:  SUBGRIDTREENODE_CELLSPARCITYLIMIT

    /// <summary>
    /// The number of passes to increment a cell pass array by, when constructing filtered cell pass arrays
    /// </summary>
    public const int kVlpdpsNodeCellPassAggregationListSizeIncrementDefault = 100; // overide with:  VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT

    /// <summary>
    /// Defines the maximum number of cell passes permitted in a subgrid segment before that segment will be split
    /// </summary>
    public const int kVlpdSubGridSegmentPassCountLimitDefault = 15000;     // overide with:  VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT

    /// <summary>
    /// Defines the maximum number of cell passes that may occur within a single cell within a segment
    /// </summary>
    public const int kVlpdSubGridMaxSegmentCellPassesLimitDefault = 250;   // overide with: VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT
    
    /// <summary>
    /// Record the result of each segment cleave operation to the log
    /// </summary>
    public const bool kSegmentCleavingOperationsToLogDefault = false;       // overide with: SEGMENTCLEAVINGOOPERATIONS_TOLOG

    /// <summary>
    /// Records meta data about items to the log as they are written into the persistent store
    /// </summary>
    public const bool kItemsPersistedViaDataPersistorToLogDefault = false;  // overide with: ITEMSPERSISTEDVIADATAPERSISTOR_TOLOG
    
    /// <summary>
    /// Enforces integrity on segments when they are added
    /// </summary>
    public const bool kPerformSegmentAdditionIntegrityChecksDefault = false;       // overide with: DEBUG_PERFORMSEGMENT_ADDITIONALINTEGRITYCHECKS

    /// <summary>
    /// Paints a red diagonal cross on each rendered tile to aid in confirming correct registration of rendered data
    /// </summary>
    public const bool kDebugDrawDiagonalCrossOnRenderedTilesDefault = false;      // overide with: DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES

    /// <summary>
    /// Controls notification of site model changes made in the persistent to interested listeners
    /// such as cluster processing nodes in the immutable grid
    /// </summary>
    public const bool kAdviseOtherServicesOfDataModelChangesDefault = true;       // overide with: ADVISEOTHERSERVICES_OFMODELCHANGES
    
    /// <summary>
    /// Maximum number of TAG files to processing through the aggregation/integration pipeline
    /// as a single work unit
    /// </summary>
    public const int kMaxMappedTagFilesToProcessPerAggregationEpochDefault = 20;  // overide with: MAXMAPPEDTAGFILES_TOPROCESSPERAGGREGATIONEPOCH

    /// <summary>
    /// The number of partitions configured for caches that store spatial subgrid data
    /// </summary>
    public const uint kNumPartitionsPerDataCacheDefault = 1024;                   // overide with: NUMPARTITIONS_PERDATACACHE

    /// <summary>
    /// The minimum tag file size required to contain even basic configuration
    /// </summary>
    public const int kMinTagFileLengthDefault = 100;                              // overide with:  MIN_TAGFILE_LENGTH

    /// <summary>
    /// The minimum tag file size required to contain even basic configuration
    /// </summary>
    public const bool kEnableTagFileServiceDefault = true;                       // overide with:  ENABLE_TFA_SERVICE

    /// <summary>
    /// Archive tag file (this has normally already been done prior to submission to TRex 
    /// </summary>
    public const bool kEnableTagFileArchivingDefault = false;                   // overide with: ENABLE_TAGFILE_ARCHIVING

    /// <summary>
    /// Archive metadata with tag file
    /// </summary>
    public const bool kEnableTagFileArchivingMetaDataDefault = false;            // overide with: ENABLE_TAGFILE_ARCHIVING_METADATA

    /// <summary>
    /// Cache intermediary subgrid results for reuse in subsequent requests
    /// </summary>
    public const bool kEnableGeneralSubgridResultCaching = true;                 // override with: ENABLE_GENERAL_SUBGRID_RESULT_CACHING
  }
}
