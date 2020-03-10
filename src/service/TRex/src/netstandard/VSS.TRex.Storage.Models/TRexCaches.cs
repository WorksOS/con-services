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
        private const string kSpatialSubGridDirectoryMutable = "Spatial-SubGridDirectory-Mutable";
        private const string kSpatialSubGridDirectoryImmutable = "Spatial-SubGridDirectory-Immutable";

        private const string kSpatialSubGridSegmentMutable = "Spatial-SubGridSegment-Mutable";
        private const string kSpatialSubGridSegmentImmutable = "Spatial-SubGridSegment-Immutable";

        private const string kNonSpatialMutable = "NonSpatial-Mutable";
        private const string kNonSpatialImmutable = "NonSpatial-Immutable";
  
        private const string kSiteModelMetadataCache = "SiteModelMetadata";
        private const string kSiteModelsCacheMutable = "SiteModels-Mutable";
        private const string kSiteModelsCacheImmutable = "SiteModels-Immutable";

        private const string kProductionDataExistenceMapCacheImmutable = "ProductionDataExistenceMap-Immutable";
        private const string kProductionDataExistenceMapCacheMutable = "ProductionDataExistenceMap-Mutable";

        private const string kDesignTopologyExistenceMaps = "DesignTopologyExistenceMaps";
        private const string kSiteModelChangeMapsCacheName = "SiteModelChangeMaps";

        private const string kTAGFileBufferQueueCacheName = "TAGFileBufferQueue";
        private const string kSiteModelChangeBufferQueueName = "SiteModelChangeBufferQueue";

        private const string kSegmentRetirementQueue = "SegmentRetirementQueue";

        /// <summary>
        /// Returns the name of the spatial grid cache to use to locate cell and cell pass information
        /// </summary>
        public static string SpatialSubGridDirectoryCacheName(StorageMutability mutability) => mutability == StorageMutability.Mutable ? kSpatialSubGridDirectoryMutable : kSpatialSubGridDirectoryImmutable;

        /// <summary>
        /// Returns the name of the spatial grid cache to use to locate cell and cell pass information
        /// </summary>
        public static string SpatialSubGridSegmentCacheName(StorageMutability mutability) => mutability == StorageMutability.Mutable ? kSpatialSubGridSegmentImmutable : kSpatialSubGridSegmentMutable;

        /// <summary>
        /// Returns the name of the event grid cache to use to locate machine event and other non spatial information
        /// </summary>
        public static string MutableNonSpatialCacheName() => kNonSpatialMutable;

        /// <summary>
        /// Returns the name of the spatial grid cache to use to store immutable cell and cell pass information
        /// </summary>
        /// <returns></returns>
        public static string ImmutableNonSpatialCacheName() => kNonSpatialImmutable;

        public static string SpatialCacheName(StorageMutability mutability, FileSystemStreamType streamType)
        {
          switch (streamType)
          {
            case FileSystemStreamType.ProductionDataXML:
              return SiteModelsCacheName(mutability);
            case FileSystemStreamType.SubGridDirectory:
              return SpatialSubGridDirectoryCacheName(mutability);
            case FileSystemStreamType.SubGridSegment:
              return SpatialSubGridSegmentCacheName(mutability);
            case FileSystemStreamType.SubGridExistenceMap:
              return ProductionDataExistenceMapCacheName(mutability);
            default:
              return string.Empty;
          }
        }

        public static string NonSpatialCacheName(StorageMutability mutability, FileSystemStreamType streamType)
        {
          switch (streamType)
          {
            case FileSystemStreamType.ProductionDataXML:
              return SiteModelsCacheName(mutability);
            case FileSystemStreamType.SiteModelMachineElevationChangeMap:
              return SiteModelChangeMapsCacheName();
            default:
              return mutability == StorageMutability.Mutable ? MutableNonSpatialCacheName() : ImmutableNonSpatialCacheName();
          }
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
        /// <returns></returns>
        public static string SiteModelChangeBufferQueueCacheName() => kSiteModelChangeBufferQueueName;
  }
}
