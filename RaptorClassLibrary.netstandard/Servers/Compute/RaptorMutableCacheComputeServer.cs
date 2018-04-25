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
    public class RaptorMutableCacheComputeServer : RaptorIgniteServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string PersistentCacheStoreLocation = @"C:\Temp\RaptorIgniteData\Mutable";

        public override void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            base.ConfigureRaptorGrid(cfg);

            cfg.IgniteInstanceName = RaptorGrids.RaptorMutableGridName();
            //cfg.ConsistentId = "SpatialDivision"+RaptorServerConfig.Instance().SpatialSubdivisionDescriptor.ToString();

            cfg.JvmInitialMemoryMb = 512; // Set to minimum advised memory for Ignite grid JVM of 512Mb
            cfg.JvmMaxMemoryMb = 1 * 1024; // Set max to 1Gb
            cfg.UserAttributes = new Dictionary<string, object>
            {
                { "Owner", RaptorGrids.RaptorMutableGridName() },
                { "SpatialDivision", RaptorServerConfig.Instance().SpatialSubdivisionDescriptor }
            };

            // Configure the Ignite persistence layer to store our data
            // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
            cfg.DataStorageConfiguration = new DataStorageConfiguration
            {
                PageSize = DataRegions.DEFAULT_MUTABLE_DATA_REGION_PAGE_SIZE,

                StoragePath = Path.Combine(PersistentCacheStoreLocation, "Persistence"),
                WalArchivePath = Path.Combine(PersistentCacheStoreLocation, "WalArchive"),
                WalPath = Path.Combine(PersistentCacheStoreLocation, "WalStore"),

                DefaultDataRegionConfiguration = new DataRegionConfiguration
                {
                    Name = DataRegions.DEFAULT_MUTABLE_DATA_REGION_NAME,
                    InitialSize = 128 * 1024 * 1024,  // 128 MB
                    MaxSize = 1L * 1024 * 1024 * 1024,  // 1 GB                               

                    PersistenceEnabled = true                    
                },

                // Establish a separate data region for the TAG file buffer queue
                DataRegionConfigurations = new List<DataRegionConfiguration>
                {
                    new DataRegionConfiguration
                    {
                        Name = DataRegions.TAG_FILE_BUFFER_QUEUE_DATA_REGION,
                        InitialSize = 128 * 1024 * 1024,  // 128 MB
                        MaxSize = 128 * 1024 * 1024,  // 128 MB

                        PersistenceEnabled = true
                    }
                }
            };

            //cfg.JvmOptions = new List<string>() { "-DIGNITE_QUIET=false" };

            cfg.DiscoverySpi = new TcpDiscoverySpi()
            {
                LocalAddress = "127.0.0.1",
                LocalPort = 48500, // + (int)RaptorServerConfig.Instance().SpatialSubdivisionDescriptor

                IpFinder = new TcpDiscoveryStaticIpFinder()
                {
                    Endpoints = new [] { "127.0.0.1:48500..48509" }
                }
            };

            cfg.CommunicationSpi = new TcpCommunicationSpi()
            {
                LocalAddress = "127.0.0.1",
                LocalPort = 48100,
            };

            cfg.Logger = new IgniteLog4NetLogger(Log);

            // Set an Ignite metrics heartbeat of 10 seconds 
            cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10);

            cfg.PublicThreadPoolSize = 50;

            cfg.BinaryConfiguration = new BinaryConfiguration(typeof(TestQueueItem));
        }

        public override void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialMutableCache(cfg);

            cfg.Name = RaptorCaches.MutableNonSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;

            // Non-spatial (event) data is replicated to all nodes for local access
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public override ICache<string, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return mutableRaptorGrid.GetOrCreateCache<string, byte[]>(CacheCfg);
        }

        public override void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureMutableSpatialCache(cfg);

            cfg.Name = RaptorCaches.MutableSpatialCacheName();
//            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
            cfg.KeepBinaryInStore = false;

            cfg.Backups = 0;

            // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
            cfg.CacheMode = CacheMode.Partitioned;

            // Configure the function that maps subgrid data into the affinity map for the nodes in the grid
            cfg.AffinityFunction = new RaptorSpatialAffinityFunction(role: ServerRoles.TAG_PROCESSING_NODE, numPartitions: (int)RaptorConfig.numTAGFileProcessingDivisions);
        }

        public override ICache<SubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return mutableRaptorGrid.GetOrCreateCache<SubGridSpatialAffinityKey, byte[]>(CacheCfg);
        }

        public static bool SetGridActive(string gridName)
        {
            // Get an ignite reference to the named grid
            IIgnite ignite = Ignition.TryGetIgnite(gridName);

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
            IgniteConfiguration cfg = new IgniteConfiguration();
            ConfigureRaptorGrid(cfg);

            Log.InfoFormat($"Creating new Ignite node for {cfg.IgniteInstanceName}");

            try
            {
                mutableRaptorGrid = Ignition.Start(cfg);
            }
            catch (Exception e)
            {
                Log.Info($"Exception during creation of new Ignite node:\n {e}");
                throw;
            }
            finally
            {
                Log.Info("Completed creation of new Ignite node");
            }

            // Wait until the grid is active
            ActivatePersistentGridServer.Instance().WaitUntilGridActive(RaptorGrids.RaptorMutableGridName());

            // Add the mutable Spatial & NonSpatial caches

            CacheConfiguration CacheCfg = new CacheConfiguration();
            ConfigureNonSpatialMutableCache(CacheCfg);           
            NonSpatialMutableCache = InstantiateRaptorCacheReference(CacheCfg);

            CacheCfg = new CacheConfiguration();
            ConfigureMutableSpatialCache(CacheCfg);
            SpatialMutableCache = InstantiateSpatialCacheReference(CacheCfg);
        }

        /// <summary>
        /// Constructor for the Raptor cache compute server node. Responsible for starting all Ignite services and creating the grid
        /// and cache instance in preparation for client access by business logic running on the node.
        /// </summary>
        public RaptorMutableCacheComputeServer()
        {
            if (mutableRaptorGrid == null)
            {
                StartRaptorGridCacheNode();
            }
        }
    }
}
