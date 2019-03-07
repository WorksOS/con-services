using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Designs.Servers.Client
{
    public class CalculateDesignElevationsServer : ImmutableClientServer
    {
        public CalculateDesignElevationsServer() : base(ServerRoles.DESIGN_PROFILER)
        {
        }
    }
}
