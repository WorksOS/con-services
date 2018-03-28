using System.Drawing;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Requests;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
using VSS.TRex.Rendering.Abstractions;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class RaptorTileRenderingServer : RaptorApplicationServiceServer
    {
        /// <summary>
        /// Default no-arg constructor that creates a server with the default Application Service role and the specialise tile rendering role.
        /// </summary>
        public RaptorTileRenderingServer() : base(new string[] { RaptorApplicationServiceServer.DEFAULT_ROLE, ServerRoles.TILE_RENDERING_NODE })
        {
        }

        /// <summary>
        /// Creates a new instance of a tile rendering server. 
        /// </summary>
        /// <returns></returns>
        public static RaptorTileRenderingServer NewInstance()
        {
            return new RaptorTileRenderingServer();
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
