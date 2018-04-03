using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// A base class for deriving server and client instances that interact with the Ignite In Memory Data Grid
    /// </summary>
    public abstract class RaptorIgniteServer : IDisposable
    {
        /// <summary>
        /// The mutable Ignite grid reference maintained by this server instance
        /// </summary>
        protected IIgnite mutableRaptorGrid = null;

        /// <summary>
        /// The immutable Ignite grid reference maintained by this server instance
        /// </summary>
        protected IIgnite immutableRaptorGrid = null;

        protected static ICache<string, byte[]> NonSpatialMutableCache = null;
        protected static ICache<string, byte[]> NonSpatialImmutableCache = null;
        protected static ICache<SubGridSpatialAffinityKey, byte[]> SpatialMutableCache = null;
        protected static ICache<SubGridSpatialAffinityKey, byte[]> SpatialImmutableCache = null;

        /// <summary>
        /// A unique identifier for this server that may be used by business logic executing on other nodes in the grid to locate it if needed for messaging
        /// </summary>
        public string RaptorNodeID = string.Empty;

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

        public abstract ICache<string, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg);

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

        public abstract ICache<SubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (mutableRaptorGrid != null)
                    {
                        Ignition.Stop(mutableRaptorGrid.Name, false);
                    }

                    if (immutableRaptorGrid != null)
                    {
                        Ignition.Stop(immutableRaptorGrid.Name, false);
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RaptorIgniteServer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
