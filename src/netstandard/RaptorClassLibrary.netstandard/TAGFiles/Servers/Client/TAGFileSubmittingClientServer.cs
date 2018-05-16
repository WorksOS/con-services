using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.TAGFiles.Servers.Client
{
    /// <summary>
    /// A server instance that represents the activities of the TAG file submitter as a client of the core cache/compute grids
    /// in the Ignite cluster. 
    /// </summary>
    public class TAGFileSubmittingClientServer : MutableClientServer
    {
        /// <summary>
        /// Default constructor for the TAG file submitter server. This server configures the local environment to be suitable for
        /// TAG file submitting operations.
        /// </summary>
        public TAGFileSubmittingClientServer() : base(ServerRoles.TAG_PROCESSING_NODE_CLIENT)
        {
        }
    }
}
