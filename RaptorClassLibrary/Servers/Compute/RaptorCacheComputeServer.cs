using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Storage;
using Apache.Ignite.Core.Cluster;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// Defines a representation of a server responsible for performing Raptor related compute operations using
    /// the Ignite In Memory Data Grid
    /// </summary>
    public class RaptorCacheComputeServer : RaptorIgniteServer
    {
        public virtual void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            cfg.GridName = RaptorGrids.RaptorGridName();
            cfg.JvmMaxMemoryMb = 4000;
            cfg.UserAttributes = new Dictionary<String, object>();
            cfg.UserAttributes.Add("Owner", RaptorGrids.RaptorGridName());
        }

        public virtual void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            cfg.Name = RaptorCaches.MutableNonSpatialCacheName();
            cfg.CopyOnRead = false;
            cfg.KeepBinaryInStore = false;
            cfg.MemoryMode = CacheMemoryMode.OnheapTiered;
            cfg.CacheStoreFactory = new RaptorCacheStoreFactory();
            cfg.ReadThrough = true;
            cfg.WriteThrough = true;
            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
            cfg.EvictionPolicy = new LruEvictionPolicy()
            {
                MaxMemorySize = 250000000
            };
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public virtual void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
        {
            cfg.Name = RaptorCaches.ImmutableNonSpatialCacheName();
            cfg.CopyOnRead = false;
            cfg.KeepBinaryInStore = false;
            cfg.MemoryMode = CacheMemoryMode.OnheapTiered;
            cfg.CacheStoreFactory = new RaptorCacheStoreFactory();
            cfg.ReadThrough = true;
            cfg.WriteThrough = true;
            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
            cfg.EvictionPolicy = new LruEvictionPolicy()
            {
                MaxMemorySize = 250000000
            };
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public virtual ICache<String, MemoryStream> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return raptorGrid.GetOrCreateCache<String, MemoryStream>(CacheCfg);
        }

        public virtual void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            cfg.Name = RaptorCaches.MutableSpatialCacheName();
            cfg.CopyOnRead = false;
            cfg.KeepBinaryInStore = false;
            cfg.MemoryMode = CacheMemoryMode.OnheapTiered;
            cfg.CacheStoreFactory = new RaptorCacheStoreFactory();
            cfg.ReadThrough = true;
            cfg.WriteThrough = true;
            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
            cfg.EvictionPolicy = new LruEvictionPolicy()
            {
                MaxMemorySize = 3000000000
            };
            cfg.Backups = 0;
            cfg.CacheMode = CacheMode.Partitioned;
            cfg.AffinityFunction = new RaptorSpatialAffinityFunction();
        }

        public virtual void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
        {
            cfg.Name = RaptorCaches.ImmutableSpatialCacheName();
            cfg.CopyOnRead = false;
            cfg.KeepBinaryInStore = false;
            cfg.MemoryMode = CacheMemoryMode.OnheapTiered;
            cfg.CacheStoreFactory = new RaptorCacheStoreFactory();
            cfg.ReadThrough = true;
            cfg.WriteThrough = true;
            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
            cfg.EvictionPolicy = new LruEvictionPolicy()
            {
                MaxMemorySize = 1000000000
            };
            cfg.Backups = 0;
            cfg.CacheMode = CacheMode.Partitioned;
            cfg.AffinityFunction = new RaptorSpatialAffinityFunction();
        }

        public virtual ICache<String, MemoryStream> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return raptorGrid.GetOrCreateCache<String, MemoryStream>(CacheCfg);
        }

        public virtual void StartRaptorGridCacheNode()
        {
            IgniteConfiguration cfg = new IgniteConfiguration();
            ConfigureRaptorGrid(cfg);

            raptorGrid = Ignition.Start(cfg);

            CacheConfiguration CacheCfg = null;

            // Add the mutable and immutable NonSpatial caches

            CacheCfg = new CacheConfiguration();
            ConfigureNonSpatialMutableCache(CacheCfg);
            NonSpatialMutableCache = InstantiateRaptorCacheReference(CacheCfg);

            CacheCfg = new CacheConfiguration();
            ConfigureNonSpatialImmutableCache(CacheCfg);
            NonSpatialImmutableCache = InstantiateRaptorCacheReference(CacheCfg);

            // Add the mutable and immutable NonSpatial caches

            CacheCfg = new CacheConfiguration();
            ConfigureMutableSpatialCache(CacheCfg);
            SpatialMutableCache = InstantiateSpatialCacheReference(CacheCfg);

            CacheCfg = new CacheConfiguration();
            ConfigureImmutableSpatialCache(CacheCfg);
            SpatialImmutableCache = InstantiateSpatialCacheReference(CacheCfg);
        }

        /// <summary>
        /// Constructor for the Raptor cache compute server node. Responsible for starting all Ignite services and creating the grid
        /// and cache instance in preparation for client access by business logic running on the node.
        /// </summary>
        public RaptorCacheComputeServer()
        {
            if (raptorGrid == null)
            {
                StartRaptorGridCacheNode();
            }
        }
    }
}
