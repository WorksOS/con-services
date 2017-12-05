using System.Drawing;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Requests;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class RaptorTileRenderingServer : RaptorApplicationServiceServer
    {
        public RaptorTileRenderingServer() : base()
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
        public Bitmap RenderTile(TileRenderRequestArgument argument)
        {
            TileRenderRequest request = new TileRenderRequest();

            return request.Execute(argument);
        }
    }
}
