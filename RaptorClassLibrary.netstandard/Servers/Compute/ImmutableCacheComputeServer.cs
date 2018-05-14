using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Log4Net;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Queues;
using VSS.VisionLink.Raptor.Servers.Client;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// Defines a representation of a server responsible for performing Raptor related compute operations using
    /// the Ignite In Memory Data Grid
    /// </summary>
    public class ImmutableCacheComputeServer : IgniteServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string PersistentCacheStoreLocation = @"C:\Temp\RaptorIgniteData\Immutable";

        public override void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            base.ConfigureRaptorGrid(cfg);

            cfg.IgniteInstanceName = RaptorGrids.RaptorImmutableGridName();

            cfg.JvmInitialMemoryMb = 512; // Set to minimum advised memory for Ignite grid JVM of 512Mb
            cfg.JvmMaxMemoryMb = 1 * 1024; // Set max to 1Gb
            cfg.UserAttributes = new Dictionary<string, object>
            {
                { "Owner", RaptorGrids.RaptorImmutableGridName() }
            };

            // Configure the Ignite 2.1 persistence layer to store our data
            // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
            cfg.DataStorageConfiguration = new DataStorageConfiguration
            {
                PageSize = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_PAGE_SIZE,

                StoragePath = Path.Combine(PersistentCacheStoreLocation, "Persistence"),
                WalArchivePath = Path.Combine(PersistentCacheStoreLocation, "WalArchive"),
                WalPath = Path.Combine(PersistentCacheStoreLocation, "WalStore"),

                DefaultDataRegionConfiguration = new DataRegionConfiguration
                {
                    Name = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_NAME,
                    InitialSize = 128 * 1024 * 1024,  // 128 MB
                    MaxSize = 1L * 1024 * 1024 * 1024,  // 1 GB                               

                    PersistenceEnabled = true                    
                }
            };

            //cfg.JvmOptions = new List<string>() { "-DIGNITE_QUIET=false" };

            cfg.DiscoverySpi = new TcpDiscoverySpi()
            {
                LocalAddress = "127.0.0.1",
                LocalPort = 47500, 

                IpFinder = new TcpDiscoveryStaticIpFinder()
                {
                    Endpoints = new [] { "127.0.0.1:47500..47509" }
                }
            };

            cfg.CommunicationSpi = new TcpCommunicationSpi()
            {
                LocalAddress = "127.0.0.1",
                LocalPort = 47100,
            };

            cfg.Logger = new IgniteLog4NetLogger(Log);

            // Set an Ignite metrics heartbeat of 10 seconds 
            cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10);

            cfg.PublicThreadPoolSize = 50;

            cfg.BinaryConfiguration = new BinaryConfiguration(typeof(TestQueueItem));
        }

        public override void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialImmutableCache(cfg);

            cfg.Name = RaptorCaches.ImmutableNonSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;

            // Non-spatial (event) data is replicated to all nodes for local access
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public override ICache<NonSpatialAffinityKey, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return immutableRaptorGrid.GetOrCreateCache<NonSpatialAffinityKey, byte[]>(CacheCfg);
        }

        public override void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureImmutableSpatialCache(cfg);

            cfg.Name = RaptorCaches.ImmutableSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;
            cfg.Backups = 0;

            // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
            cfg.CacheMode = CacheMode.Partitioned;

            // Configure the function that maps subgrid data into the affinity map for the nodes in the grid
            cfg.AffinityFunction = new ImmutableSpatialAffinityFunction();
        }

        public override ICache<SubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return immutableRaptorGrid.GetOrCreateCache<SubGridSpatialAffinityKey, byte[]>(CacheCfg);
        } 

        public static bool SetGridActive(string gridName)
        {
            // Get an ignite reference to the named grid
            IIgnite ignite = RaptorGridFactory.Grid(gridName);

            // If the grid exists, and it is not active, then set it to active
            if (ignite != null && !ignite.GetCluster().IsActive())
            {
                ignite.GetCluster().SetActive(true);

                Log.InfoFormat("Set grid '{0}' to active.", gridName);

                return true;
            }
            else
            {
                Log.InfoFormat("Grid '{0}' is not available or is already active.", gridName);

                return ignite != null && ignite.GetCluster().IsActive();
            }
        }

        public void StartRaptorGridCacheNode()
        {
            Log.InfoFormat("Creating new Ignite node");

            IgniteConfiguration cfg = new IgniteConfiguration();
            ConfigureRaptorGrid(cfg);

            Log.InfoFormat($"Creating new Ignite node for {cfg.IgniteInstanceName}");

            try
            {
                immutableRaptorGrid = Ignition.Start(cfg);
            }
            catch (Exception e)
            {
                Log.Info($"Exception during creation of new Ignite node:\n {e}");
            }
            finally
            {
                Log.InfoFormat("Completed creation of new Ignite node");
            }

            // Wait until the grid is active
            ActivatePersistentGridServer.Instance().WaitUntilGridActive(RaptorGrids.RaptorImmutableGridName());

            // Add the immutable Spatial & NonSpatial caches

            CacheConfiguration CacheCfg = new CacheConfiguration();
            ConfigureNonSpatialImmutableCache(CacheCfg);
            NonSpatialImmutableCache = InstantiateRaptorCacheReference(CacheCfg);

            CacheCfg = new CacheConfiguration();
            ConfigureImmutableSpatialCache(CacheCfg);
            SpatialImmutableCache = InstantiateSpatialCacheReference(CacheCfg);
        }

        /// <summary>
        /// Constructor for the Raptor cache compute server node. Responsible for starting all Ignite services and creating the grid
        /// and cache instance in preparation for client access by business logic running on the node.
        /// </summary>
        public ImmutableCacheComputeServer()
        {
            if (immutableRaptorGrid == null)
            {
                StartRaptorGridCacheNode();
            }
        }
    }
}
