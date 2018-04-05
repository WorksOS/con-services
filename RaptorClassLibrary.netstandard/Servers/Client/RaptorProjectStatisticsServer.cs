namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// A server type supporting requests relating to project statistics
    /// </summary>
    public class RaptorProjectStatisticsServer : RaptorMutableClientServer
    {
        public RaptorProjectStatisticsServer() : base(ServerRoles.ASNODE)
        {

        }
    }
}
