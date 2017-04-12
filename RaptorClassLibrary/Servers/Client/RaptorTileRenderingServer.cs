using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Requests;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class RaptorTileRenderingServer : RaptorApplicationServiceServer
    {
        /// <summary>
        /// Creates a new instance of a tile rendering server. 
        /// </summary>
        /// <returns></returns>
        public static RaptorTileRenderingServer NewInstance()
        {
            return new RaptorTileRenderingServer();
        }

        /// <summary>
        /// Render a thematic tile bitmap according the the given arguments
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Bitmap RenderTile(TileRenderRequestArgument argument)
        {
            return TileRenderRequest.Execute(argument);
        }
    }
}
