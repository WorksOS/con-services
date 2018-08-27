using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.Servers.Client
{
    /// <summary>
    /// A server type supporting requests relating to project statistics
    /// </summary>
    public class ProjectStatisticsServer : MutableClientServer
    {
        public ProjectStatisticsServer() : base(ServerRoles.ASNODE)
        {

        }
    }
}
