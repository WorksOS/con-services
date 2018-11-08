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
using System.Reflection;
using Apache.Ignite.Core.Deployment;
using VSS.TRex.Logging;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Storage.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;

namespace VSS.TRex.Servers.Client
{
    /// <summary>
    /// Defines a representation of a client able to request TRex related compute operations using
    /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
    /// </summary>
    public class ImmutableClientServer : IgniteServer, IImmutableClientServer
  {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Constructor that creates a new server instance with a single role
    /// </summary>
    /// <param name="role"></param>
    public ImmutableClientServer(string role) : this(new[] { role })
    {
    }

    /// <summary>
    /// Constructor that creates a new server instance with a set of roles
    /// </summary>
    /// <param name="roles"></param>
    public ImmutableClientServer(string[] roles)
    {
      if (immutableTRexGrid == null)
      {
        // Attempt to attach to an already existing Ignite instance
        Log.LogInformation("Getting Immmutable grid");
        immutableTRexGrid = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable);
        Log.LogInformation($"Got {immutableTRexGrid?.Name}");

        // If there was no connection obtained, attempt to create a new instance
        if (immutableTRexGrid == null)
        {
          string roleNames = roles.Aggregate("|", (s1, s2) => s1 + s2 + "|");

          TRexNodeID = Guid.NewGuid().ToString();

          //Log.LogInformation($"Creating new Ignite node with Roles = {roleNames} & TRexNodeId = {TRexNodeID}");

          IgniteConfiguration cfg = new IgniteConfiguration
          {
            IgniteInstanceName = TRexGrids.ImmutableGridName(),
            ClientMode = true,

            JvmOptions = new List<string>() { "-DIGNITE_QUIET=false", "-Djava.net.preferIPv4Stack=true" },

            JvmInitialMemoryMb = 512, // Set to minimum advised memory for Ignite grid JVM of 512Mb
            JvmMaxMemoryMb = 1 * 1024, // Set max to 1Gb

            UserAttributes = new Dictionary<string, object>()
                        {
                            { "TRexNodeId", TRexNodeID }
                        },


            Logger = new TRexIgniteLogger(Logger.CreateLogger("ImmutableClientServer")),

            // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
            DataStorageConfiguration = new DataStorageConfiguration
            {
              WalMode = WalMode.Fsync,
              PageSize = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_PAGE_SIZE,

              DefaultDataRegionConfiguration = new DataRegionConfiguration
              {
                Name = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_NAME,
                InitialSize = 128 * 1024 * 1024,  // 128 MB
                MaxSize = 256 * 1024 * 1024,  // 256 Mb
                PersistenceEnabled = false
              },
            },

            // Set an Ignite metrics heartbeat of 10 seconds
            MetricsLogFrequency = new TimeSpan(0, 0, 0, 10),

            PublicThreadPoolSize = 50,

            PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.Disabled

            // BinaryConfiguration = new BinaryConfiguration(typeof(TestQueueItem))
          };

          foreach (string roleName in roles)
          {
            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{roleName}", "True");
          }


          bool.TryParse(Environment.GetEnvironmentVariable("IS_KUBERNETES"), out bool isKubernetes);
          cfg = isKubernetes ? setKubernetesIgniteConfiguration(cfg) : setLocalIgniteConfiguration(cfg);

          try
          {
            immutableTRexGrid = Ignition.Start(cfg);
          }
          catch (Exception e)
          {
            Log.LogInformation($"Creation of new Ignite node with Role = {roleNames} & TRexNodeId = {TRexNodeID} failed with exception {e}");
            throw;
          }
          finally
          {
            Log.LogInformation($"Completed creation of new Ignite node with Role = {roleNames} & TRexNodeId = {TRexNodeID}");
          }
        }
      }
    }

    private IgniteConfiguration setKubernetesIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\igniteKubeConfig.xml";

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalPort = 47100,
      };
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

      // Enforce using only the LocalHost interface
      cfg.DiscoverySpi = new TcpDiscoverySpi()
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47500,

        IpFinder = new TcpDiscoveryStaticIpFinder()
        {
          Endpoints = new[] { "127.0.0.1:47500..47509" }
        }
      };

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47100,
      };
      return cfg;
    }

    public override ICache<INonSpatialAffinityKey, byte[]> InstantiateTRexCacheReference(CacheConfiguration CacheCfg)
    {
      return immutableTRexGrid.GetCache<INonSpatialAffinityKey, byte[]>(CacheCfg.Name);
    }

    public override ICache<ISubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
    {
      return immutableTRexGrid.GetCache<ISubGridSpatialAffinityKey, byte[]>(CacheCfg.Name);
    }
  }
}
