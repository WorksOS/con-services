using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Servers
{
    /// <summary>
    /// A base class for deriving server and client instances that interact with the Ignite In Memory Data Grid
    /// </summary>
    public abstract class IgniteServer : IDisposable
    {
        /// <summary>
        /// The mutable Ignite grid reference maintained by this server instance
        /// </summary>
        protected IIgnite mutableTRexGrid = null;

        /// <summary>
        /// The immutable Ignite grid reference maintained by this server instance
        /// </summary>
        protected IIgnite immutableTRexGrid = null;

        protected static ICache<INonSpatialAffinityKey, byte[]> NonSpatialMutableCache = null;
        protected static ICache<INonSpatialAffinityKey, byte[]> NonSpatialImmutableCache = null;
        protected static ICache<ISubGridSpatialAffinityKey, byte[]> SpatialMutableCache = null;
        protected static ICache<ISubGridSpatialAffinityKey, byte[]> SpatialImmutableCache = null;

        /// <summary>
        /// A unique identifier for this server that may be used by business logic executing on other nodes in the grid to locate it if needed for messaging
        /// </summary>
        public string TRexNodeID = string.Empty;

        /// <summary>
        /// Base configuration for the grid
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureTRexGrid(IgniteConfiguration cfg)
        {
        }

        /// <summary>
        /// Base configuration for the mutable non-spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.MUTABLE_NONSPATIAL_DATA_REGION;
        }

        /// <summary>
        /// Base configuration for the immutable non-spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION;
        }

        public abstract ICache<INonSpatialAffinityKey, byte[]> InstantiateTRexCacheReference(CacheConfiguration CacheCfg);

        /// <summary>
        /// Base configuration for the mutable spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.MUTABLE_SPATIAL_DATA_REGION;
        }

        /// <summary>
        /// Base configuration for the immutable spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.IMMUTABLE_SPATIAL_DATA_REGION;
        }

        public abstract ICache<ISubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg);

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
          DIContext.Obtain<ITRexGridFactory>()?.StopGrids();
        }
    }
}
