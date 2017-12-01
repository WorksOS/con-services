using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers.Compute;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// A base class for deriving server and client instances that interact with the Ignite In Memory Data Grid
    /// </summary>
    public abstract class RaptorIgniteServer
    {
        /// <summary>
        /// The Ignite grid reference maintained by this server instance
        /// </summary>
        protected IIgnite raptorGrid = null;

        protected static ICache<String, byte[]> NonSpatialMutableCache = null;
        protected static ICache<String, byte[]> NonSpatialImmutableCache = null;
        protected static ICache<String, byte[]> SpatialMutableCache = null;
        protected static ICache<String, byte[]> SpatialImmutableCache = null;

        /// <summary>
        /// A unique identifier for this server that may be used by business logic executing on other nodes in the grid to locate it if needed for messaging
        /// </summary>
        public string RaptorNodeID = String.Empty;

        /// <summary>
        /// Permits configuration of server specific parameters that influence the initialisation of the server type
        /// </summary>
        public virtual void SetupServerSpecificConfiguration()
        {
        }

        /// <summary>
        /// Default constructor for the Raptor Ignite Server. This must be called in the base() constructor chain to ensure
        /// the server operating environment is correctly configured before instantiation of the server inner workings
        /// </summary>
        public RaptorIgniteServer()
        {
            SetupServerSpecificConfiguration();
        }

        /// <summary>
        /// Base configuration for the grid
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureRaptorGrid(IgniteConfiguration cfg)
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

        public abstract ICache<String, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg);

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

        public abstract ICache<String, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg);
    }
}
