using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// A server instance that represente the activities of the TAG file processor as a client of the core cache/compute grids
    /// in the Ignite cluster. The TAG file processor is currently a client server in that it interacts with the cache information on the 
    /// primary Ignite cluster cache.
    /// </summary>
    public class TAGFileProcessingServer : RaptorClientServer
    {
        /// <summary>
        /// Default constructor for the TAG file processor server. This server configures the local environment to be suitable for
        /// TAG file processing operations.
        /// </summary>
        public TAGFileProcessingServer() : base("TAGProc")
        {
            // Ensure that the TAG file processor only reads data from the mutable cache to prevent the possibly lossy conversion from mutable to
            // immutable forms of the cached data polluting the result of processing additional TAG file data into the database
            RaptorServerConfig.Instance().ReadFromImmutableDataCaches = false;
        }
    }
}
