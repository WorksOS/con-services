namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// Represents a server instance that client servers implmenting application service type capabilities such as
    /// tile rendering should descend from
    /// </summary>
    public class RaptorApplicationServiceServer : RaptorImmutableClientServer
    {
        public const string DEFAULT_ROLE = ServerRoles.ASNODE;

        public RaptorApplicationServiceServer() : base(new [] { DEFAULT_ROLE })
        {
        }

        public RaptorApplicationServiceServer(string [] roles) : base(roles)
        {
        }
    }
}
