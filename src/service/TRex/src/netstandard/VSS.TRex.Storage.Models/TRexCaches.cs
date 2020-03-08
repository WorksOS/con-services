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
        private const string kSpatialMutable = "Spatial-Mutable";
  //      private const string kSpatialImmutable = "Spatial-Immutable";
        private const string kSpatialImmutableCompressed = "Spatial-Immutable-Compressed";

        private const string kNonSpatialMutable = "NonSpatial-Mutable";
  //      private const string kNonSpatialImmutable = "NonSpatial-Immutable";
        private const string kNonSpatialImmutableCompressed = "NonSpatial-Immutable"; // Same as compressed as there is currently no distinction

        private const string kSiteModelMetadataCache = "SiteModelMetadataCache";
        private const string kSiteModelsCacheMutable = "SiteModelsCache-Mutable";
        private const string kSiteModelsCacheImmutable = "SiteModelsCache-Immutable";

        private const string kDesignTopologyExistenceMaps = "DesignTopologyExistenceMaps";
        private const string kSiteModelChangeMapsCacheName = "SiteModelChangeMapsCache";

        private const string kTAGFileBufferQueueCacheName = "TAGFileBufferQueue";
        private const string kSiteModelChangeBufferQueueName = "SiteModelChangeBufferQueue";

        /// <summary>
        /// Returns the name of the spatial grid cache to use to locate cell and cell pass information
        /// </summary>
        public static string MutableSpatialCacheName() => kSpatialMutable;

        /// <summary>
        /// Returns the name of the spatial grid cache to use to store mutable cell and cell pass information
        /// </summary>
        /// <returns></returns>
        public static string ImmutableSpatialCacheName() => kSpatialImmutableCompressed;
    
        /// <summary>
        /// Returns the name of the event grid cache to use to locate machine event and other non spatial information
        /// </summary>
        public static string MutableNonSpatialCacheName() => kNonSpatialMutable;

        /// <summary>
        /// Returns the name of the spatial grid cache to use to store immutable cell and cell pass information
        /// </summary>
        /// <returns></returns>
        public static string ImmutableNonSpatialCacheName() => kNonSpatialImmutableCompressed;

        public static string SpatialCacheName(StorageMutability mutability, FileSystemStreamType streamType)
        {
          if (streamType == FileSystemStreamType.ProductionDataXML)
            return SiteModelsCacheName(mutability);

          return mutability == StorageMutability.Mutable ? MutableSpatialCacheName() : ImmutableSpatialCacheName();
        }

        public static string NonSpatialCacheName(StorageMutability mutability, FileSystemStreamType streamType)
        {
          if (streamType == FileSystemStreamType.ProductionDataXML)
            return SiteModelsCacheName(mutability);

          if (streamType == FileSystemStreamType.SiteModelMachineElevationChangeMap)
            return SiteModelChangeMapsCacheName();

          return mutability == StorageMutability.Mutable ? MutableNonSpatialCacheName() : ImmutableNonSpatialCacheName();
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
        public static string SegmentRetirementQueueCacheName() => "SegmentRetirementQueue";

        /// <summary>
        /// Name of the cache holding queued & buffered TAG files awaiting processing
        /// </summary>
        /// <returns></returns>
        public static string SiteModelChangeBufferQueueCacheName() => kSiteModelChangeBufferQueueName;
  }
}
