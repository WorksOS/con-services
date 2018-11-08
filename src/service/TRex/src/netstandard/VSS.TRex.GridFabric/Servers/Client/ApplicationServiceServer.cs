using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.Servers.Client
{
    /// <summary>
    /// Represents a server instance that client servers implmenting application service type capabilities such as
    /// tile rendering should descend from
    /// </summary>
    public class ApplicationServiceServer : ImmutableClientServer
    {
        public const string DEFAULT_ROLE = ServerRoles.ASNODE;
        public const string DEFAULT_ROLE_CLIENT = ServerRoles.ASNODE_CLIENT;

        public ApplicationServiceServer() : base(new [] { DEFAULT_ROLE })
        {
        }

        public ApplicationServiceServer(string [] roles) : base(roles)
        {
        }
    }
}
