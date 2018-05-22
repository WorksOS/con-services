using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Servers.Compute
{
    /// <summary>
    /// Defines a representation of a server responsible for performing TRex related compute operations using
    /// the Ignite In Memory Data Grid
    /// </summary>
    public class TagProcComputeServer : MutableCacheComputeServer
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// A client reference to the immutable data grid for the TAG file processing logic to write immutable versions
        /// of the data being processed from TAG files into.
        /// </summary>
        private ImmutableClientServer ImmutableClientServer;

        public override void ConfigureTRexGrid(IgniteConfiguration cfg)
        {
            base.ConfigureTRexGrid(cfg);

            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.TAG_PROCESSING_NODE}", "True");
        }

        public override void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialMutableCache(cfg);
        }

        public override void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureMutableSpatialCache(cfg);
        }

        /// <summary>
        /// Constructor for the TRex cache compute server node. Responsible for starting all Ignite services and creating the grid
        /// and cache instance in preparation for client access by business logic running on the node.
        /// </summary>
        public TagProcComputeServer()
        {
            ImmutableClientServer = new ImmutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
        }
    }
}
