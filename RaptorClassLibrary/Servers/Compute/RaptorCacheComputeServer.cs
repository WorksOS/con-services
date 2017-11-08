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
using Apache.Ignite.Log4Net;
using log4net;
using System.Reflection;
using Apache.Ignite.Core.PersistentStore;
using VSS.VisionLink.Raptor.Servers.Client;
using System.Threading;
using Apache.Ignite.Core.Discovery.Tcp;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// Defines a representation of a server responsible for performing Raptor related compute operations using
    /// the Ignite In Memory Data Grid
    /// </summary>
    public class RaptorCacheComputeServer : RaptorIgniteServer
    {
        private const string PersistentCacheStoreLocation = @"C:\Temp\RaptorIgniteData";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            base.ConfigureRaptorGrid(cfg);

            // cfg.GridName = RaptorGrids.RaptorGridName();
            cfg.IgniteInstanceName = RaptorGrids.RaptorGridName();// + RaptorServerConfig.Instance().SpatialSubdivisionDescriptor.ToString();
            cfg.JvmInitialMemoryMb = 512; // Set to minimum advised memory for Ignite grid JVM of 512Mb
            cfg.JvmMaxMemoryMb = 1 * 1024; // Set max to 1Gb
            cfg.UserAttributes = new Dictionary<String, object>
            {
                { "Owner", RaptorGrids.RaptorGridName() }
            };

            // Configure the Ignite 2.1 persistence layer to store our data

            cfg.PersistentStoreConfiguration = new PersistentStoreConfiguration()
            {
                //MetricsEnabled = true,
                PersistentStorePath = Path.Combine(PersistentCacheStoreLocation, "Persistence"),
                WalArchivePath = Path.Combine(PersistentCacheStoreLocation, "WalArchive"),
                WalStorePath = Path.Combine(PersistentCacheStoreLocation, "WalStore"),
            };

            //cfg.JvmOptions = new List<string>() { "-DIGNITE_QUIET=false" };

            // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
            cfg.MemoryConfiguration = new MemoryConfiguration()
            {
                SystemCacheMaxSize = (long)1 * 1024 * 1024 * 1024,
                DefaultMemoryPolicyName = "defaultPolicy",
                MemoryPolicies = new[]
                {
                    new MemoryPolicyConfiguration
                    {
                        Name = "defaultPolicy",
                        InitialSize = 128 * 1024 * 1024,  // 128 MB
                        MaxSize = 1L * 1024 * 1024 * 1024  // 1 GB
                    }
                }
            };

            cfg.DiscoverySpi = new TcpDiscoverySpi()
            {
                LocalAddress = "127.0.0.1" //,

                // Make sure each individual subdivision uses a different port number - useful when running clusters on a local system
//                LocalPort = 47500 + (int)RaptorServerConfig.Instance().SpatialSubdivisionDescriptor
            };

            cfg.Logger = new IgniteLog4NetLogger(Log);

            // Set an Ignite metrics heartbeat of 10 seconds 
            cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10);

            cfg.PublicThreadPoolSize = 50;            
        }

        public override void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialMutableCache(cfg);

            cfg.Name = RaptorCaches.MutableNonSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;

//            cfg.CacheStoreFactory = new RaptorCacheStoreFactory(false, true);
//            cfg.ReadThrough = true;
//            cfg.WriteThrough = true;
//            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
//            cfg.EvictionPolicy = new LruEvictionPolicy()
//            {
//                MaxMemorySize = 100000000   // 100Mb
//            };

            // Non-spatial (event) data is replicated to all nodes for local access
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public override void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialImmutableCache(cfg);

            cfg.Name = RaptorCaches.ImmutableNonSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;

//            cfg.CacheStoreFactory = new RaptorCacheStoreFactory(false, false);
//            cfg.ReadThrough = true;
//            cfg.WriteThrough = true;
//            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
//            cfg.EvictionPolicy = new LruEvictionPolicy()
//            {
//                MaxMemorySize = 250000000   // 250Mb
//            };

            // Non-spatial (event) data is replicated to all nodes for local access
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public override ICache<String, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return raptorGrid.GetOrCreateCache<String, byte[]>(CacheCfg);
        }

        public override void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureMutableSpatialCache(cfg);

            cfg.Name = RaptorCaches.MutableSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;

//            cfg.CacheStoreFactory = new RaptorCacheStoreFactory(true, true);
//            cfg.ReadThrough = true;
//            cfg.WriteThrough = true;
//            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
//            cfg.EvictionPolicy = new LruEvictionPolicy()
//            {
//                MaxMemorySize = 500000000 // 500Mb
//            };
            cfg.Backups = 0;

            // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
            cfg.CacheMode = CacheMode.Partitioned;

            // Configure the function that maps subgrid data into the affinity map for the nodes in the grid
            cfg.AffinityFunction = new RaptorSpatialAffinityFunction();
        }

        public override void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureImmutableSpatialCache(cfg);

            cfg.Name = RaptorCaches.ImmutableSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;

//            cfg.CacheStoreFactory = new RaptorCacheStoreFactory(true, false);
//            cfg.ReadThrough = true;
//            cfg.WriteThrough = true;
//            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
//            cfg.EvictionPolicy = new LruEvictionPolicy()
//            {
//                MaxMemorySize = 1000000000   // 1Gb
//            };
            cfg.Backups = 0;

            // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
            cfg.CacheMode = CacheMode.Partitioned;

            // Configure the function that maps subgrid data into the affinity map for the nodes in the grid
            cfg.AffinityFunction = new RaptorSpatialAffinityFunction();
        }

        public override ICache<String, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return raptorGrid.GetOrCreateCache<String, byte[]>(CacheCfg);
        }

        public static bool SetGridActive(string gridName)
        {
            // Get an ignite reference to the named grid
            IIgnite ignite = Ignition.TryGetIgnite(gridName);

            // If the grid exists, and it is not active, then set it to active
            if (ignite != null && !ignite.IsActive())
            {
                ignite.SetActive(true);

                Log.InfoFormat("Set grid '{0}' to active.", gridName);

                return true;
            }
            else
            {
                Log.InfoFormat("Grid '{0}' is not available or is already active.", gridName);

                return ignite != null && ignite.IsActive();
            }
        }

        public virtual void StartRaptorGridCacheNode()
        {
            Log.InfoFormat("Creating new Ignite node");

            IgniteConfiguration cfg = new IgniteConfiguration();
            ConfigureRaptorGrid(cfg);

            try
            {
                raptorGrid = Ignition.Start(cfg);
            }
            catch (Exception e)
            {
                Log.InfoFormat("Creation of new Ignite node", e);
            }
            finally
            {
                Log.InfoFormat("Completed creation of new Ignite node");
            }

            // Wait until the grid is active
            ActivatePersistentGridServer.Instance().WaitUntilGridActive(RaptorGrids.RaptorGridName());

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
        public RaptorCacheComputeServer() : base()
        {
            if (raptorGrid == null)
            {
                StartRaptorGridCacheNode();
            }
        }
    }
}
