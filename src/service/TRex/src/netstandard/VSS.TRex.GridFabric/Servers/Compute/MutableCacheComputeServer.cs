using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Deployment;
using VSS.TRex.Common;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Logging;
using VSS.TRex.Servers.Client;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.Servers.Compute
{
  /// <summary>
  /// Defines a representation of a server responsible for performing TRex related compute operations using
  /// the Ignite In Memory Data Grid
  /// </summary>
  public class MutableCacheComputeServer : IgniteServer
  {
    private static readonly ILogger Log = Logger.CreateLogger<MutableCacheComputeServer>();

    /// <summary>
    /// Constructor for the TRex cache compute server node. Responsible for starting all Ignite services and creating the grid
    /// and cache instance in preparation for client access by business logic running on the node.
    /// </summary>
    public MutableCacheComputeServer()
    {
      Log.LogDebug($"PersistentCacheStoreLocation is: {TRexServerConfig.PersistentCacheStoreLocation}");
      if (mutableTRexGrid == null)
      {
        StartTRexGridCacheNode();
      }
    }

    public override void ConfigureTRexGrid(IgniteConfiguration cfg)
    {
      //cfg.SpringConfigUrl = @".\igniteMutableKubeConfig.xml";
      base.ConfigureTRexGrid(cfg);

      cfg.IgniteInstanceName = TRexGrids.MutableGridName();

      cfg.JvmInitialMemoryMb = 512; // Set to minimum advised memory for Ignite grid JVM of 512Mb
      cfg.JvmMaxMemoryMb = 2 * 1024; // Set max to 2Gb
      cfg.UserAttributes = new Dictionary<string, object>
            {
                { "Owner", TRexGrids.MutableGridName() }
            };

      // Configure the Ignite persistence layer to store our data
      // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
      cfg.DataStorageConfiguration = new DataStorageConfiguration
      {
        WalMode = WalMode.Fsync,
        PageSize = DataRegions.DEFAULT_MUTABLE_DATA_REGION_PAGE_SIZE,

        StoragePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable", "Persistence"),
        WalArchivePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable", "WalArchive"),
        WalPath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable", "WalStore"),

        DefaultDataRegionConfiguration = new DataRegionConfiguration
        {
          Name = DataRegions.DEFAULT_MUTABLE_DATA_REGION_NAME,
          InitialSize = 128 * 1024 * 1024,  // 128 MB
          MaxSize = 2L * 1024 * 1024 * 1024,  // 2 GB

          PersistenceEnabled = true
        },

        // Establish a separate data region for the TAG file buffer queue
        DataRegionConfigurations = new List<DataRegionConfiguration>
                {
                    new DataRegionConfiguration
                    {
                        Name = DataRegions.TAG_FILE_BUFFER_QUEUE_DATA_REGION,
                        InitialSize = 128 * 1024 * 1024,  // 128 MB
                        MaxSize = 256 * 1024 * 1024,  // 128 MB

                        PersistenceEnabled = true
                    }
                }
      };

      cfg.CacheConfiguration = new List<CacheConfiguration>
      {

      };

      Log.LogInformation($"cfg.DataStorageConfiguration.StoragePath={cfg.DataStorageConfiguration.StoragePath}");
      Log.LogInformation($"cfg.DataStorageConfiguration.WalArchivePath={cfg.DataStorageConfiguration.WalArchivePath}");
      Log.LogInformation($"cfg.DataStorageConfiguration.WalPath={cfg.DataStorageConfiguration.WalPath}");

      cfg.JvmOptions = new List<string>() {
        "-DIGNITE_QUIET=false",
        "-Djava.net.preferIPv4Stack=true" };


      cfg.Logger = new TRexIgniteLogger(Logger.CreateLogger("MutableCacheComputeServer"));

      // Set an Ignite metrics heartbeat of 10 seconds
      cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10);

      cfg.PublicThreadPoolSize = 50;

      cfg.PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.Disabled;

      cfg.BinaryConfiguration = new BinaryConfiguration
      {
        Serializer = new BinarizableSerializer()
      }; 

      bool.TryParse(Environment.GetEnvironmentVariable("IS_KUBERNETES"), out bool isKubernetes);
      cfg = isKubernetes ? setKubernetesIgniteConfiguration(cfg) : setLocalIgniteConfiguration(cfg);
      cfg.WorkDirectory = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable");
    }

    private IgniteConfiguration setKubernetesIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\ignitePersistantMutableKubeConfig.xml";

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalPort = 48100,
      };
      return cfg;
    }

    private IgniteConfiguration setLocalIgniteConfiguration(IgniteConfiguration cfg)
    {
      //temp
      cfg.SpringConfigUrl = @".\mutablePersistence.xml";


      //TODO this should not be here but will do for the moment
      TRexServerConfig.PersistentCacheStoreLocation = Path.Combine(Path.GetTempPath(), "TRexIgniteData");

      // Enforce using only the LocalHost interface
      cfg.DiscoverySpi = new TcpDiscoverySpi()
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 48500,

        IpFinder = new TcpDiscoveryStaticIpFinder()
        {
          Endpoints = new[] { "127.0.0.1:48500..48509" }
        }
      };

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 48100,
      };
      return cfg;
    }

    public override void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
    {
      //base.ConfigureNonSpatialMutableCache(cfg);

      //cfg.Name = TRexCaches.MutableNonSpatialCacheName();
      ////            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
      //cfg.KeepBinaryInStore = false;

      //// Non-spatial (event) data is replicated to all nodes for local access
      //cfg.CacheMode = CacheMode.Partitioned;

      //// Note: The AffinityFunction is longer supplied as the ProjectUID (Guid) member of the
      //// NonSpatialAffinityKey struct is marked with the [AffinityKeyMapped] attribute. For Partitioned caches
      //// this means the values are spread amongst the servers per the default
      //cfg.AffinityFunction = new MutableNonSpatialAffinityFunction();

      //cfg.Backups = 0;
    }

    public override ICache<INonSpatialAffinityKey, byte[]> InstantiateTRexCacheReference(CacheConfiguration CacheCfg)
    {
      return mutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, byte[]>(CacheCfg);
    }

    public override void ConfigureMutableSpatialCache(CacheConfiguration cfg)
    {
      //TODO This is handled by SPRING at the moment

      base.ConfigureMutableSpatialCache(cfg);

      cfg.Name = TRexCaches.MutableSpatialCacheName();
      //            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
      cfg.KeepBinaryInStore = false;

      cfg.Backups = 0;

      // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
      cfg.CacheMode = CacheMode.Partitioned;

      // Configure the function that maps subgrid data into the affinity map for the nodes in the grid
      cfg.AffinityFunction = new MutableSpatialAffinityFunction();
    }

    public override ICache<ISubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
    {
      return mutableTRexGrid.GetOrCreateCache<ISubGridSpatialAffinityKey, byte[]>(CacheCfg);
    }

    public void ConfigureTAGFileBufferQueueCache(CacheConfiguration cfg)
    {
      cfg.Name = TRexCaches.TAGFileBufferQueueCacheName();

      cfg.KeepBinaryInStore = true;

      // Replicate the maps across nodes
      cfg.CacheMode = CacheMode.Partitioned;

      cfg.AffinityFunction = new MutableNonSpatialAffinityFunction();

      // No backups for now
      cfg.Backups = 0;

      cfg.DataRegionName = DataRegions.TAG_FILE_BUFFER_QUEUE_DATA_REGION;
    }

    public void InstantiateTAGFileBufferQueueCacheReference(CacheConfiguration CacheCfg)
    {
      mutableTRexGrid.GetOrCreateCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(CacheCfg);
    }

    public static bool SetGridActive(string gridName)
    {
      // Get an ignite reference to the named grid
      IIgnite ignite = Ignition.TryGetIgnite(gridName);

      // If the grid exists, and it is not active, then set it to active
      if (ignite != null && !ignite.GetCluster().IsActive())
      {
        ignite.GetCluster().SetActive(true);

        Log.LogInformation($"Set grid '{gridName}' to active.");

        return true;
      }
      else
      {
        Log.LogInformation($"Grid '{gridName}' is not available or is already active.");

        return ignite != null && ignite.GetCluster().IsActive();
      }
    }

    public void StartTRexGridCacheNode()
    {
      IgniteConfiguration cfg = new IgniteConfiguration();
      ConfigureTRexGrid(cfg);

      Log.LogInformation($"Creating new Ignite node for {cfg.IgniteInstanceName}");

      try
      {
        mutableTRexGrid = Ignition.Start(cfg);
      }
      catch (Exception e)
      {
        Log.LogInformation($"Exception during creation of new Ignite node:\n {e}");
        throw;
      }
      finally
      {
        Log.LogInformation("Completed creation of new Ignite node");
      }

      // Wait until the grid is active
      ActivatePersistentGridServer.Instance().WaitUntilGridActive(TRexGrids.MutableGridName());

      // Add the mutable Spatial & NonSpatial caches

      //CacheConfiguration CacheCfg = new CacheConfiguration();
      //ConfigureNonSpatialMutableCache(CacheCfg);
      var nonSpatialCacheConfiguration = mutableTRexGrid.GetConfiguration().CacheConfiguration.First(x => x.Name.Equals(TRexCaches.MutableNonSpatialCacheName()));
      NonSpatialMutableCache = InstantiateTRexCacheReference(nonSpatialCacheConfiguration);

      //CacheCfg = new CacheConfiguration();
      //ConfigureMutableSpatialCache(CacheCfg);
      //SpatialMutableCache = InstantiateSpatialCacheReference(CacheCfg);
      var spatialCacheConfiguration = mutableTRexGrid.GetConfiguration().CacheConfiguration.First(x => x.Name.Equals(TRexCaches.MutableSpatialCacheName()));
      SpatialMutableCache = InstantiateSpatialCacheReference(spatialCacheConfiguration);

      //CacheCfg = new CacheConfiguration();
      //ConfigureTAGFileBufferQueueCache(CacheCfg);
      //InstantiateTAGFileBufferQueueCacheReference(CacheCfg);
      var tagCacheConfiguration = mutableTRexGrid.GetConfiguration().CacheConfiguration.First(x => x.Name.Equals(TRexCaches.TAGFileBufferQueueCacheName()));
      InstantiateTAGFileBufferQueueCacheReference(tagCacheConfiguration);
    }

  }
}
