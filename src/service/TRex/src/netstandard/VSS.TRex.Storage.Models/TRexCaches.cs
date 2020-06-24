using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Storage.Caches
{
    /// <summary>
    /// Spatial grid cache provides logic to determine which of the spatial data grid caches an application should read data from
    /// depending on it settings in TRexServerConfig
    /// </summary>
    public static class TRexCaches
    {
        public const string kSpatialSubGridDirectoryMutable = "Spatial-SubGridDirectory-Mutable";
        public const string kSpatialSubGridDirectoryImmutable = "Spatial-SubGridDirectory-Immutable";

        public const string kSpatialSubGridSegmentMutable = "Spatial-SubGridSegment-Mutable";
        public const string kSpatialSubGridSegmentImmutable = "Spatial-SubGridSegment-Immutable";

        public const string kNonSpatialMutable = "NonSpatial-Mutable";
        public const string kNonSpatialImmutable = "NonSpatial-Immutable";

        public const string kSiteModelMetadataCache = "SiteModelMetadata";
        public const string kSiteModelsCacheMutable = "SiteModels-Mutable";
        public const string kSiteModelsCacheImmutable = "SiteModels-Immutable";

        public const string kProductionDataExistenceMapCacheImmutable = "ProductionDataExistenceMap-Immutable";
        public const string kProductionDataExistenceMapCacheMutable = "ProductionDataExistenceMap-Mutable";

        public const string kDesignTopologyExistenceMaps = "DesignTopologyExistenceMaps";
        public const string kSiteModelChangeMapsCacheName = "SiteModelChangeMaps";

        public const string kTAGFileBufferQueueCacheName = "TAGFileBufferQueue";
        public const string kSiteModelChangeBufferQueueName = "SiteModelChangeBufferQueue";

        public const string kSegmentRetirementQueue = "SegmentRetirementQueue";

        public const string kSiteModelRebuilderMetaDataCacheName = "SiteModelRebuilderMetaData";

        public const string kSiteModelRebuilderFileKeyCollectionsCacheName = "SiteModelRebuilderFileKeyCollections";

        /// <summary>
        /// Returns the name of the spatial grid cache to use to locate cell and cell pass information
        /// </summary>
        public static string SpatialSubGridDirectoryCacheName(StorageMutability mutability) => mutability == StorageMutability.Mutable ? kSpatialSubGridDirectoryMutable : kSpatialSubGridDirectoryImmutable;

        /// <summary>
        /// Returns the name of the spatial grid cache to use to locate cell and cell pass information
        /// </summary>
        public static string SpatialSubGridSegmentCacheName(StorageMutability mutability) => mutability == StorageMutability.Mutable ? kSpatialSubGridSegmentMutable : kSpatialSubGridSegmentImmutable;

        /// <summary>
        /// Returns the name of the event grid cache to use to locate machine event and other non spatial information
        /// </summary>
        public static string MutableNonSpatialCacheName() => kNonSpatialMutable;

        /// <summary>
        /// Returns the name of the spatial grid cache to use to store immutable cell and cell pass information
        /// </summary>
        public static string ImmutableNonSpatialCacheName() => kNonSpatialImmutable;

        public static string SpatialCacheName(StorageMutability mutability, FileSystemStreamType streamType)
        {
          return streamType switch
          {
            FileSystemStreamType.ProductionDataXML => SiteModelsCacheName(mutability),
            FileSystemStreamType.SubGridDirectory => SpatialSubGridDirectoryCacheName(mutability),
            FileSystemStreamType.SubGridSegment => SpatialSubGridSegmentCacheName(mutability),
            FileSystemStreamType.SubGridExistenceMap => ProductionDataExistenceMapCacheName(mutability),
            _ => string.Empty,
          };
        }

        public static string NonSpatialCacheName(StorageMutability mutability, FileSystemStreamType streamType)
        {
          return streamType switch
          {
            FileSystemStreamType.ProductionDataXML => SiteModelsCacheName(mutability),
            FileSystemStreamType.SiteModelMachineElevationChangeMap => SiteModelChangeMapsCacheName(),
            _ => mutability == StorageMutability.Mutable ? MutableNonSpatialCacheName() : ImmutableNonSpatialCacheName(),
          };
        }

        /// <summary>
        /// Returns the name of of the design topology existence maps
        /// </summary>
        /// <returns></returns>
        public static string SiteModelMetadataCacheName() => kSiteModelMetadataCache;
   
        /// <summary>
        /// Returns the name of of the site models cache
        /// </summary>
        /// <returns></returns>
        public static string SiteModelsCacheName(StorageMutability mutability) => mutability == StorageMutability.Immutable ? kSiteModelsCacheImmutable : kSiteModelsCacheMutable;
        
        /// <summary>
        /// Returns the name of of the production data existence maps cache
        /// </summary>
        /// <returns></returns>
        public static string ProductionDataExistenceMapCacheName(StorageMutability mutability) => mutability == StorageMutability.Immutable ? kProductionDataExistenceMapCacheImmutable : kProductionDataExistenceMapCacheMutable;

        /// <summary>
        /// Returns the name of of the design topology existence maps
        /// </summary>
        /// <returns></returns>
        public static string DesignTopologyExistenceMapsCacheName() => kDesignTopologyExistenceMaps;

        /// <summary>
        /// Returns the name of the cache responsible for holding generic quantities where the site model and the machine are
        /// the core identifier for that information. These
        /// </summary>
        /// <returns></returns>
        public static string SiteModelChangeMapsCacheName() => kSiteModelChangeMapsCacheName;

        /// <summary>
        /// Name of the cache holding queued & buffered TAG files awaiting processing
        /// </summary>
        /// <returns></returns>
        public static string TAGFileBufferQueueCacheName() => kTAGFileBufferQueueCacheName;

        /// <summary>
        /// Name of the cache holding the segments in the data model that need to be retired due to being
        /// replaced by small cloven segments as a result of TAG file processing
        /// </summary>
        public static string SegmentRetirementQueueCacheName() => kSegmentRetirementQueue;

        /// <summary>
        /// Name of the cache holding queued & buffered TAG files awaiting processing
        /// </summary>
        public static string SiteModelChangeBufferQueueCacheName() => kSiteModelChangeBufferQueueName;

        /// <summary>
        /// Name of the cache holding meta data relate to site model rebulding operations
        /// </summary>
        public static string SiteModelRebuilderMetaDataCacheName() => kSiteModelRebuilderMetaDataCacheName;

        /// <summary>
        /// Name of the cache holding collections of tag file keys describing the set of TAG file to be processed when rebulding a project
        /// </summary>
        public static string SiteModelRebuilderFileKeyCollectionsCacheName() => kSiteModelRebuilderFileKeyCollectionsCacheName;
    }
}
