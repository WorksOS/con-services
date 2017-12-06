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
    }
}
