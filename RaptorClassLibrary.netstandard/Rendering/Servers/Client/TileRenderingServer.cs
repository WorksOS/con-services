using VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Requests;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Responses;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class TileRenderingServer : ApplicationServiceServer
    {
        /// <summary>
        /// Default no-arg constructor that creates a server with the default Application Service role and the specialise tile rendering role.
        /// </summary>
        public TileRenderingServer() : base(new [] { ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.TILE_RENDERING_NODE })
        {
        }

        public TileRenderingServer(string [] roles) : base(roles)
        {
        }

        /// <summary>
        /// Creates a new instance of a tile rendering server. 
        /// </summary>
        /// <returns></returns>
        public static TileRenderingServer NewInstance(string [] roles)
        {
            return new TileRenderingServer(roles);
        }

        /// <summary>
        /// Render a thematic tile bitmap according to the given arguments
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public TileRenderResponse RenderTile(TileRenderRequestArgument argument)
        {
            TileRenderRequest request = new TileRenderRequest();

            return request.Execute(argument);
        }
    }
}
