using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class TileRenderingServer : ApplicationServiceServer, ITileRenderingServer
    {
        /// <summary>
        /// Default no-arg constructor that creates a server with the default Application Service role and the specialise tile rendering role.
        /// </summary>
        public TileRenderingServer() : base(new [] { ServerRoles.TILE_RENDERING_NODE })
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
