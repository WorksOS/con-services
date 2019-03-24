using VSS.TRex.Exports.Patches;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Exports.Servers.Client
{
  /// <summary>
  /// The server used to house tile rendering services
  /// </summary>
  public class PatchRequestServer : ApplicationServiceServer
  {
    /// <summary>
    /// Default no-arg constructor that creates a server with the default Application Service role and the specialize tile rendering role.
    /// </summary>
    public PatchRequestServer() : this(new[] {ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.PATCH_REQUEST_ROLE})
    {
    }

    public PatchRequestServer(string[] roles) : base(roles)
    {
    }
  }
}
