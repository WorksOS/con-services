using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class TileRenderingServer : ApplicationServiceServer, ITileRenderingServer
    {
        /// <summary>
        /// Default no-arg constructor that creates a server with the default Application Service role and the specialize tile rendering role.
        /// </summary>
        public TileRenderingServer() : this(new [] { ServerRoles.TILE_RENDERING_NODE })
        {
        }

        public TileRenderingServer(string [] roles) : base(roles)
        {
        }
    }
}
