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
using System.Reflection;
using Apache.Ignite.Core.Deployment;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.Servers.Client;
using VSS.TRex.Storage;

namespace VSS.TRex.Servers.Compute
{
  /// <summary>
  /// Defines a representation of a server responsible for performing TRex related compute operations using
  /// the Ignite In Memory Data Grid
  /// </summary>
  public class ImmutableCacheComputeServer : IgniteServer
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Constructor for the TRex cache compute server node. Responsible for starting all Ignite services and creating the grid
    /// and cache instance in preparation for client access by business logic running on the node.
    /// </summary>
    public ImmutableCacheComputeServer()
    {
      Console.WriteLine("PersistentCacheLocation:" + TRexConfig.PersistentCacheStoreLocation);
      Console.WriteLine($"Log is: {Log}");
      Log.LogDebug($"PersistentCacheStoreLocation: {TRexConfig.PersistentCacheStoreLocation}");
      if (immutableTRexGrid == null)
      {
        StartTRexGridCacheNode();
      }
    }

    public override void ConfigureTRexGrid(IgniteConfiguration cfg)
    {
      base.ConfigureTRexGrid(cfg);

      cfg.SpringConfigUrl = @".\igniteKubeConfig.xml";

      cfg.IgniteInstanceName = TRexGrids.ImmutableGridName();

      cfg.JvmInitialMemoryMb = 512; // Set to minimum advised memory for Ignite grid JVM of 512Mb
      cfg.JvmMaxMemoryMb = 1 * 1024; // Set max to 1Gb
      cfg.UserAttributes = new Dictionary<string, object>
            {
                { "Owner", TRexGrids.ImmutableGridName() }
            };

      // Configure the Ignite 2.1 persistence layer to store our data
      // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
      cfg.DataStorageConfiguration = new DataStorageConfiguration
      {
        PageSize = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_PAGE_SIZE,

        StoragePath = Path.Combine(TRexConfig.PersistentCacheStoreLocation, "Immutable", "Persistence"),
        WalArchivePath = Path.Combine(TRexConfig.PersistentCacheStoreLocation, "Immutable", "WalArchive"),
        WalPath = Path.Combine(TRexConfig.PersistentCacheStoreLocation, "Immutable", "WalStore"),

        DefaultDataRegionConfiguration = new DataRegionConfiguration
        {
          Name = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_NAME,
          InitialSize = 128 * 1024 * 1024,  // 128 MB
          MaxSize = 1L * 1024 * 1024 * 1024,  // 1 GB                               

          PersistenceEnabled = true
        }
      };

      Log.LogInformation($"cfg.DataStorageConfiguration.StoragePath={cfg.DataStorageConfiguration.StoragePath}");
      Log.LogInformation($"cfg.DataStorageConfiguration.WalArchivePath={cfg.DataStorageConfiguration.WalArchivePath}");
      Log.LogInformation($"cfg.DataStorageConfiguration.WalPath={cfg.DataStorageConfiguration.WalPath}");

      cfg.JvmOptions = new List<string>() { "-DIGNITE_QUIET=false" };

      cfg.SpringConfigUrl = @".\igniteKubeConfig.xml";

      //cfg.DiscoverySpi = new TcpDiscoverySpi()
      //{
      //  LocalAddress = "127.0.0.1",
      //  LocalPort = 47500,

      //  IpFinder = new TcpDiscoveryStaticIpFinder()
      //  {
      //    Endpoints = new[] { "127.0.0.1:47500..47509" }
      //  }
      //};

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        //LocalAddress = "127.0.0.1",
        LocalPort = 47100,
      };

      cfg.Logger = new TRexIgniteLogger(Logger.CreateLogger("ImmutableCacheComputeServer"));

      // Set an Ignite metrics heartbeat of 10 seconds 
      cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10);

      cfg.PublicThreadPoolSize = 50;

      cfg.PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.Disabled;


      //cfg.BinaryConfiguration = new BinaryConfiguration(typeof(TestQueueItem));
    }

    public override void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
    {
      base.ConfigureNonSpatialImmutableCache(cfg);

      cfg.Name = TRexCaches.ImmutableNonSpatialCacheName();
      //            cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
      cfg.KeepBinaryInStore = false;

      // Non-spatial (event) data is replicated to all nodes for local access
      cfg.CacheMode = CacheMode.Replicated;
      cfg.Backups = 0;
    }

    public override ICache<NonSpatialAffinityKey, byte[]> InstantiateTRexCacheReference(CacheConfiguration CacheCfg)
    {
      Console.WriteLine($"CacheConfig is: {CacheCfg}");
      Console.WriteLine($"immutableTRexGrid is : {immutableTRexGrid}");

      return immutableTRexGrid.GetOrCreateCache<NonSpatialAffinityKey, byte[]>(CacheCfg);
    }

    public override void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
    {
      base.ConfigureImmutableSpatialCache(cfg);

      cfg.Name = TRexCaches.ImmutableSpatialCacheName();
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
      return immutableTRexGrid.GetOrCreateCache<SubGridSpatialAffinityKey, byte[]>(CacheCfg);
    }

    public static bool SetGridActive(string gridName)
    {
      // Get an ignite reference to the named grid
      IIgnite ignite = TRexGridFactory.Grid(gridName);

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
      Log.LogInformation("Creating new Ignite node");

      IgniteConfiguration cfg = new IgniteConfiguration();
      ConfigureTRexGrid(cfg);

      Log.LogInformation($"Creating new Ignite node for {cfg.IgniteInstanceName}");

      try
      {
        Console.WriteLine($"Creating new Ignite node for {cfg.IgniteInstanceName}");
        immutableTRexGrid = Ignition.Start(cfg);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Exception during creation of new Ignite node:\n {e}");
        Log.LogError($"Exception during creation of new Ignite node:\n {e}");
      }
      finally
      {
        Log.LogInformation("Completed creation of new Ignite node");
      }

      // Wait until the grid is active
      ActivatePersistentGridServer.Instance().WaitUntilGridActive(TRexGrids.ImmutableGridName());

      // Add the immutable Spatial & NonSpatial caches

      CacheConfiguration CacheCfg = new CacheConfiguration();
      ConfigureNonSpatialImmutableCache(CacheCfg);
      NonSpatialImmutableCache = InstantiateTRexCacheReference(CacheCfg);

      CacheCfg = new CacheConfiguration();
      ConfigureImmutableSpatialCache(CacheCfg);
      SpatialImmutableCache = InstantiateSpatialCacheReference(CacheCfg);
    }


  }
}
