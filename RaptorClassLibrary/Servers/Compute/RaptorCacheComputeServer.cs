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
            cfg.GridName = "Raptor";
            cfg.JvmMaxMemoryMb = 1000;
            cfg.UserAttributes = new Dictionary<String, object>();
            cfg.UserAttributes.Add("Owner", "Raptor");
        }

        public virtual void ConfigureRaptorCache(CacheConfiguration cfg)
        {
            cfg.Name = "DataModels";
            cfg.CopyOnRead = false;
            cfg.KeepBinaryInStore = false;
            cfg.MemoryMode = CacheMemoryMode.OnheapTiered;
            cfg.CacheStoreFactory = new RaptorCacheStoreFactory();
            cfg.ReadThrough = true;
            cfg.WriteThrough = true;
            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
            cfg.EvictionPolicy = new LruEvictionPolicy() { MaxMemorySize = 2000000000 };
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public virtual ICache<String, MemoryStream> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return raptorGrid.GetOrCreateCache<String, MemoryStream>(CacheCfg);
        }

        public virtual void ConfigureSpatialCache(CacheConfiguration cfg)
        {
            cfg.Name = "DataModels";
            cfg.CopyOnRead = false;
            cfg.KeepBinaryInStore = false;
            cfg.MemoryMode = CacheMemoryMode.OnheapTiered;
            cfg.CacheStoreFactory = new RaptorCacheStoreFactory();
            cfg.ReadThrough = true;
            cfg.WriteThrough = true;
            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
            cfg.EvictionPolicy = new LruEvictionPolicy() { MaxMemorySize = 2000000000 };
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
            //            raptorGrid = Ignition.TryGetIgnite("Raptor");

            CacheConfiguration CacheCfg = null;

            CacheCfg = new CacheConfiguration();
            ConfigureRaptorCache(CacheCfg);

            // Add a cache to Ignite
            raptorCache = InstantiateRaptorCacheReference(CacheCfg);

            CacheCfg = new CacheConfiguration();
            ConfigureSpatialCache(CacheCfg);

            // Add a cache to Ignite
            spatialCache = InstantiateSpatialCacheReference(CacheCfg);
        }

        public RaptorCacheComputeServer()
        {
            if (raptorGrid == null)
            {
                StartRaptorGridCacheNode();
            }
        }
    }
}
