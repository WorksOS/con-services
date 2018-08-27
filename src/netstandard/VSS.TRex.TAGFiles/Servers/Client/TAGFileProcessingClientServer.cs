using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.TAGFiles.Servers.Client
{
    /// <summary>
    /// A server instance that represente the activities of the TAG file processor as a client of the core cache/compute grids
    /// in the Ignite cluster. The TAG file processor is currently a client server in that it interacts with the cache information on the 
    /// primary Ignite cluster cache.
    /// </summary>
    public class TAGFileProcessingClientServer : MutableClientServer
    {
        /// <summary>
        /// Default constructor for the TAG file processor server. This server configures the local environment to be suitable for
        /// TAG file processing operations.
        /// </summary>
        public TAGFileProcessingClientServer() : base(ServerRoles.TAG_PROCESSING_NODE_CLIENT)
        {
        }
    }
}
