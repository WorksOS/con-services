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
using System.Linq;
using System.Reflection;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Queues;
using VSS.TRex.Storage;

namespace VSS.TRex.Servers.Client
{
    /// <summary>
    /// Defines a representation of a client able to request Raptor related compute operations using
    /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
    /// </summary>
    public class ImmutableClientServer : IgniteServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor that creates a new server instance with a single role
        /// </summary>
        /// <param name="role"></param>
        public ImmutableClientServer(string role) : this(new [] { role })
        {
        }

        /// <summary>
        /// Constructor that creates a new server instance with a set of roles
        /// </summary>
        /// <param name="roles"></param>
        public ImmutableClientServer(string [] roles)
        {
            if (immutableRaptorGrid == null)
            {
                // Attempt to attach to an already existing Ignite instance
                immutableRaptorGrid = TRexGridFactory.Grid(TRexGrids.ImmutableGridName());

                // If there was no connection obtained, attempt to create a new instance
                if (immutableRaptorGrid == null)
                {
                    string roleNames = roles.Aggregate("|", (s1, s2) => s1 + s2 + "|");

                    RaptorNodeID = Guid.NewGuid().ToString();

                    Log.InfoFormat("Creating new Ignite node with Roles = {0} & TRexNodeID = {1}", roleNames, RaptorNodeID);

                    IgniteConfiguration cfg = new IgniteConfiguration
                    {
                        // SpringConfigUrl = @".\RaptorIgniteConfig.xml",

                        IgniteInstanceName = TRexGrids.ImmutableGridName(),
                        ClientMode = true,

                        JvmInitialMemoryMb = 512, // Set to minimum advised memory for Ignite grid JVM of 512Mb
                        JvmMaxMemoryMb = 1 * 1024, // Set max to 1Gb

                        UserAttributes = new Dictionary<string, object>()
                        {
                            { "TRexNodeID", RaptorNodeID }
                        },

                        // Enforce using only the LocalHost interface
                        DiscoverySpi = new TcpDiscoverySpi()
                        {
                            LocalAddress = "127.0.0.1",
                            LocalPort = 47500, 

                            IpFinder = new TcpDiscoveryStaticIpFinder()
                            {
                                Endpoints = new [] { "127.0.0.1:47500..47509" }
                            }
                        },

                        CommunicationSpi = new TcpCommunicationSpi()
                        {
                            LocalAddress = "127.0.0.1",
                            LocalPort = 47100,
                        },

                        Logger = new IgniteLog4NetLogger(Log),

                        // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
                        DataStorageConfiguration = new DataStorageConfiguration
                        {
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

                        BinaryConfiguration = new BinaryConfiguration(typeof(TestQueueItem))
                    };

                    foreach (string roleName in roles)
                    {
                        cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{roleName}", "True");
                    }

                    try
                    {
                        immutableRaptorGrid = Ignition.Start(cfg);
                    }
                    catch (Exception e)
                    {
                        Log.InfoFormat("Creation of new Ignite node with Role = {0} & TRexNodeID = {1} failed with exception {2}", roleNames, RaptorNodeID, e);
                        throw;
                    }
                    finally
                    {
                        Log.InfoFormat("Completed creation of new Ignite node with Role = {0} & TRexNodeID = {1}", roleNames, RaptorNodeID);
                    }
                }
            }
        }

        public override ICache<NonSpatialAffinityKey, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return immutableRaptorGrid.GetCache<NonSpatialAffinityKey, byte[]>(CacheCfg.Name);
        }

        public override ICache<SubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return immutableRaptorGrid.GetCache<SubGridSpatialAffinityKey, byte[]>(CacheCfg.Name);
        }       
    }
}
