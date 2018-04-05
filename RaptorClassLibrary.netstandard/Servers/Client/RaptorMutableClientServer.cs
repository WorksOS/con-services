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
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Queues;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// Defines a representation of a client able to request Raptor related compute operations using
    /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
    /// </summary>
    public class RaptorMutableClientServer : RaptorIgniteServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor that creates a new server instance with a single role
        /// </summary>
        /// <param name="role"></param>
        public RaptorMutableClientServer(string role) : this(new [] { role })
        {
        }

        /// <summary>
        /// Constructor that creates a new server instance with a set of roles
        /// </summary>
        /// <param name="roles"></param>
        public RaptorMutableClientServer(string [] roles)
        {
            if (mutableRaptorGrid == null)
            {
                // Attempt to attach to an already existing Ignite instance
                mutableRaptorGrid = RaptorGridFactory.Grid(RaptorGrids.RaptorMutableGridName());

                // If there was no connection obtained, attempt to create a new instance
                if (mutableRaptorGrid == null)
                {
                    string roleNames = roles.Aggregate("|", (s1, s2) => s1 + s2 + "|");

                    RaptorNodeID = Guid.NewGuid().ToString();

                    Log.InfoFormat("Creating new Ignite node with Roles = {0} & RaptorNodeID = {1}", roleNames, RaptorNodeID);

                    IgniteConfiguration cfg = new IgniteConfiguration()
                    {
                        //                        SpringConfigUrl = @".\RaptorIgniteConfig.xml",

                        IgniteInstanceName = RaptorGrids.RaptorMutableGridName(),
                        ClientMode = true,

                        JvmInitialMemoryMb = 512, // Set to minimum advised memory for Ignite grid JVM of 512Mb
                        JvmMaxMemoryMb = 1 * 1024, // Set max to 2Gb

                        UserAttributes = new Dictionary<string, object>()
                        {
                            { "RaptorNodeID", RaptorNodeID }
                        },

                        // Enforce using only the LocalHost interface
                        DiscoverySpi = new TcpDiscoverySpi()
                        {
                            LocalAddress = "127.0.0.1",
                            LocalPort = 48500,

                            IpFinder = new TcpDiscoveryStaticIpFinder()
                            {
                                Endpoints = new [] { "127.0.0.1:48500..48509" }
                            }
                        },

                        CommunicationSpi = new TcpCommunicationSpi()
                        {
                            LocalAddress = "127.0.0.1",
                            LocalPort = 48100,
                        },

                        Logger = new IgniteLog4NetLogger(Log),

                        // Don't permit the Ignite node to use more than 1Gb RAM (handy when running locally...)
                        DataStorageConfiguration = new DataStorageConfiguration()
                        {
                            PageSize = DataRegions.DEFAULT_MUTABLE_DATA_REGION_PAGE_SIZE,

                            DefaultDataRegionConfiguration = new DataRegionConfiguration
                            {
                                Name = DataRegions.DEFAULT_MUTABLE_DATA_REGION_NAME,
                                InitialSize = 128 * 1024 * 1024,  // 128 MB
                                MaxSize = 1L * 1024 * 1024 * 1024,  // 1 GB    
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
                        mutableRaptorGrid = Ignition.Start(cfg);
                    }
                    catch (Exception e)
                    {
                        Log.InfoFormat("Creation of new Ignite node with Role = {0} & RaptorNodeID = {1} failed with exception {2}", roleNames, RaptorNodeID, e);
                    }
                    finally
                    {
                        Log.InfoFormat("Completed creation of new Ignite node with Role = {0} & RaptorNodeID = {1}", roleNames, RaptorNodeID);
                    }
                }
            }
        }

        public override ICache<string, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return mutableRaptorGrid.GetCache<string, byte[]>(CacheCfg.Name);
        }

        public override ICache<SubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return mutableRaptorGrid.GetCache<SubGridSpatialAffinityKey, byte[]>(CacheCfg.Name);
        }       
    }
}
