using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Designs.Servers.Client
{
  public class CalculateDesignElevationsServer : ApplicationServiceServer
  {
    public CalculateDesignElevationsServer() : base(new[] {ServerRoles.DESIGN_PROFILER})
    {
    }
  }
}
