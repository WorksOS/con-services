using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Profiling.Servers.Client
{
  /// <summary>
  /// A server used to house profile computation services
  /// </summary>
  public class ProfilingServer : ApplicationServiceServer
  {
    /// <summary>
    /// Default no-arg constructor that creates a server with the default Application Service role and the specialize profiling role.
    /// </summary>
    public ProfilingServer() : base(new[] { ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.ASNODE_PROFILER })
    {
    }

    public ProfilingServer(string[] roles) : base(roles)
    {
    }

    /// <summary>
    /// Creates a new instance of a profiling server. 
    /// </summary>
    /// <returns></returns>
    public static ProfilingServer NewInstance(string[] roles)
    {
      return new ProfilingServer(roles);
    }
  }
}
