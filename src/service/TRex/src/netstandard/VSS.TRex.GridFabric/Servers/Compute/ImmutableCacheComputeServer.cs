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
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Logging;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Servers.Compute
{
  /// <summary>
  /// Defines a representation of a server responsible for performing TRex related compute operations using
  /// the Ignite In Memory Data Grid
  /// </summary>
  public class ImmutableCacheComputeServer : IgniteServer
  {
    private static readonly ILogger Log = Logger.CreateLogger<ImmutableCacheComputeServer>();

    /// <summary>
    /// Constructor for the TRex cache compute server node. Responsible for starting all Ignite services and creating the grid
    /// and cache instance in preparation for client access by business logic running on the node.
    /// </summary>
    public ImmutableCacheComputeServer()
    {
      Console.WriteLine("PersistentCacheLocation:" + TRexServerConfig.PersistentCacheStoreLocation);
      Console.WriteLine($"Log is: {Log}");
      Log.LogDebug($"PersistentCacheStoreLocation: {TRexServerConfig.PersistentCacheStoreLocation}");
      if (immutableTRexGrid == null)
      {
        StartTRexGridCacheNode();
      }
    }

    public override void ConfigureTRexGrid(IgniteConfiguration cfg)
    {
      base.ConfigureTRexGrid(cfg);

      cfg.IgniteInstanceName = TRexGrids.ImmutableGridName();

      cfg.JvmOptions = new List<string>() {
        "-DIGNITE_QUIET=false",
        "-Djava.net.preferIPv4Stack=true",
        "-XX:+UseG1GC"
      };

      cfg.JvmMaxMemoryMb = 1 * 1024; // Set max to 1Gb
      cfg.UserAttributes = new Dictionary<string, object>
            {
                { "Owner", TRexGrids.ImmutableGridName() }
            };

      // Configure the Ignite 2.1 persistence layer to store our data
      // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
      cfg.DataStorageConfiguration = new DataStorageConfiguration
      {
        WalMode = WalMode.Fsync,
        PageSize = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_PAGE_SIZE,

        StoragePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable", "Persistence"),
        WalArchivePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable", "WalArchive"),
        WalPath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable", "WalStore"),

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


      bool.TryParse(Environment.GetEnvironmentVariable("IS_KUBERNETES"), out bool isKubernetes);
      cfg = isKubernetes ? setKubernetesIgniteConfiguration(cfg) : setLocalIgniteConfiguration(cfg);
      cfg.WorkDirectory = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable");

      cfg.Logger = new TRexIgniteLogger(Logger.CreateLogger("ImmutableCacheComputeServer"));

      // Set an Ignite metrics heartbeat of 10 seconds
      cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10);

      cfg.PublicThreadPoolSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(TREX_IGNITE_PUBLIC_THREAD_POOL_SIZE, DEFAULT_TREX_IGNITE_PUBLIC_THREAD_POOL_SIZE);

      cfg.PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.Disabled;

      cfg.BinaryConfiguration = new BinaryConfiguration
      {
        Serializer = new BinarizableSerializer()
      };
    }


    private IgniteConfiguration setKubernetesIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\ignitePersistentImmutableKubeConfig.xml";

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalPort = 47100,
      };

      cfg.JvmOptions.Add("-javaagent:./libs/jmx_prometheus_javaagent-0.11.0.jar=8088:prometheusConfig.yaml");

      return cfg;
    }


    /// <summary>
    /// Configures ignite for use locally i.e on developers pc
    /// </summary>
    /// <param name="cfg">Ignite configuration that is being built</param>
    /// <returns></returns>
    private IgniteConfiguration setLocalIgniteConfiguration(IgniteConfiguration cfg)
    {
      //TODO this should not be here but will do for the moment
      TRexServerConfig.PersistentCacheStoreLocation = Path.Combine(Path.GetTempPath(), "TRexIgniteData");

      cfg.SpringConfigUrl = @".\immutablePersistence.xml";

      // Enforce using only the LocalHost interface
      cfg.DiscoverySpi = new TcpDiscoverySpi
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47500,

        IpFinder = new TcpDiscoveryStaticIpFinder
        {
          Endpoints = new[] { "127.0.0.1:47500..47502" }
        }
      };

      cfg.CommunicationSpi = new TcpCommunicationSpi
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47100,
      };
      return cfg;
    }


    public override void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
    {
      base.ConfigureNonSpatialImmutableCache(cfg);

      cfg.Name = TRexCaches.ImmutableNonSpatialCacheName();
      cfg.KeepBinaryInStore = false;

      // Non-spatial (event) data is replicated to all nodes for local access
      cfg.CacheMode = CacheMode.Replicated;
      cfg.Backups = 0;
    }

    public override ICache<INonSpatialAffinityKey, byte[]> InstantiateNonSpatialTRexCacheReference(CacheConfiguration CacheCfg)
    {
      Console.WriteLine($"CacheConfig is: {CacheCfg}");
      Console.WriteLine($"immutableTRexGrid is : {immutableTRexGrid}");

      return immutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, byte[]>(CacheCfg);
    }

    public override void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
    {
      base.ConfigureImmutableSpatialCache(cfg);

      cfg.Name = TRexCaches.ImmutableSpatialCacheName();
      cfg.KeepBinaryInStore = false;
      cfg.Backups = 0;

      // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
      cfg.CacheMode = CacheMode.Partitioned;

      // Configure the function that maps sub grid data into the affinity map for the nodes in the grid
      cfg.AffinityFunction = new ImmutableSpatialAffinityFunction();
    }

    public override ICache<ISubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
    {
      return immutableTRexGrid.GetOrCreateCache<ISubGridSpatialAffinityKey, byte[]>(CacheCfg);
    }

    private void InstantiateSiteModelsCacheReference()
    {
      immutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, byte[]>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelsCacheName(StorageMutability.Immutable),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Replicated,

        // TODO: No backups for now
        Backups = 0,

        DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION
      });
    }

    /// <summary>
    /// Create the cache that holds the per project, per machine, change maps driven by TAG file ingest
    /// Note: This machine based information is distinguished from that in the non-spatial cache in that
    /// it is partitioned, rather than replicated.
    /// </summary>
    private void InstantiateSiteModelMachinesChangeMapsCacheReference()
    {
      // TODO (VSTS #85679): ######DEBUG ONLY###### Remove when change maps reactivated
      return;

      // ######DEBUG ONLY######
      immutableTRexGrid.GetOrCreateCache<ISiteModelMachineAffinityKey, byte[]>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelChangeMapsCacheName(),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Partitioned,

        // TODO: No backups for now
        Backups = 0,

        DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION,

        // Configure the function that maps the change maps to nodes in the grid
        // Note: This cache uses the mutable spatial affinity function that assigns data for 
        // a site model onto a single node. For the purposes of the immutable grid, it is helpful
        // to contain all change maps for a single site model as this simplifies the process of
        // updating those change maps in response to messages from production data ingest 
        // TODO: Make a particular Immutable version of this affinity function to improve naming consistency when we no longer require the Java affinity function implementations
        AffinityFunction = new MutableSpatialAffinityFunction()
    });
    }

    public void StartTRexGridCacheNode()
    {
      Log.LogInformation("Creating new Ignite node");

      var cfg = new IgniteConfiguration();
      ConfigureTRexGrid(cfg);

      Log.LogInformation($"Creating new Ignite node for {cfg.IgniteInstanceName}");

      try
      {
        Console.WriteLine($"Creating new Ignite node for {cfg.IgniteInstanceName}");
        immutableTRexGrid = DIContext.Obtain<ITRexGridFactory>()?.Grid(TRexGrids.ImmutableGridName(), cfg); 
      }
      finally
      {
        Log.LogInformation($"Completed creation of new Ignite node: Exists = {immutableTRexGrid != null}, Factory available = {DIContext.Obtain<ITRexGridFactory>() != null}");
      }

      // Wait until the grid is active
      DIContext.Obtain<IActivatePersistentGridServer>().WaitUntilGridActive(TRexGrids.ImmutableGridName());

      // Add the immutable Spatial & NonSpatial caches

      var CacheCfg = new CacheConfiguration();
      ConfigureNonSpatialImmutableCache(CacheCfg);
      NonSpatialImmutableCache = InstantiateNonSpatialTRexCacheReference(CacheCfg);

      //CacheCfg = new CacheConfiguration();
      var spatialCacheConfiguration = immutableTRexGrid.GetConfiguration().CacheConfiguration.First(x => x.Name.Equals(TRexCaches.ImmutableSpatialCacheName()));

      //ConfigureImmutableSpatialCache(CacheCfg);
      SpatialImmutableCache = InstantiateSpatialCacheReference(spatialCacheConfiguration);

      InstantiateSiteModelsCacheReference();
      InstantiateSiteModelMachinesChangeMapsCacheReference();
    }
  }
}
