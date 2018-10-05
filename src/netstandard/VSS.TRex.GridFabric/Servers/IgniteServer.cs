using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Servers
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
        /// Permits configuration of server specific parameters that influence the initialisation of the server type
        /// </summary>
        public virtual void SetupServerSpecificConfiguration()
        {
        }

        /// <summary>
        /// Default constructor for the TRex Ignite Server. This must be called in the base() constructor chain to ensure
        /// the server operating environment is correctly configured before instantiation of the server inner workings
        /// </summary>
        public IgniteServer()
        {
            SetupServerSpecificConfiguration();
        }

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

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (mutableTRexGrid != null)
                        Ignition.Stop(mutableTRexGrid.Name, false);

                    if (immutableTRexGrid != null)
                        Ignition.Stop(immutableTRexGrid.Name, false);
                }

                disposedValue = true;
            }
        }

        // Note: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IgniteServer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // Note: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
