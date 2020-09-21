using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.QuantizedMesh.Servers.Client
{

  public class QuantizedMeshServer : ApplicationServiceServer
  {
    /// <summary>
    /// Default no-arg constructor that creates a server with the default Application Service role and the specialize tile rendering role.
    /// </summary>
    public QuantizedMeshServer() : this(new[] { ServerRoles.QNANTIZED_MESH_NODE, ServerRoles.RECEIVES_SITEMODEL_CHANGE_EVENTS })
    {
    }

    public QuantizedMeshServer(string[] roles) : base(roles)
    {
    }
  }
}
