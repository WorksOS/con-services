using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// Defines names of various role that servers can occupy in the grid
    /// </summary>
    public static class ServerRoles
    {
        /// <summary>
        /// The name of the attribute added to a node attributes to record its role
        /// </summary>
        public static string ROLE_ATTRIBUTE_NAME = "Role";

        /// <summary>
        /// The 'PSNode' role, meaning the server is a part of subgrid clustered processing engine
        /// </summary>
        public static string PSNODE = "PSNode";

        /// <summary>
        /// The 'ASNode', application service, role, meaning the server is a part of subgrid clustered processing engine
        /// </summary>
        public static string ASNODE = "ASNode";

        /// <summary>
        /// A server responsible for processing TAG files into the production data models
        /// </summary>
        public static string TAG_PROCESING_NODE = "TagProc";

        /// <summary>
        /// A server responsible for rendering tiles from production data
        /// </summary>
        public static string TILE_RENDERING_NODE = "TileRendering";
    }
}
