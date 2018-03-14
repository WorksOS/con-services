using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// A factory class responsible for creating mutable or immutable raptor client nodes in the Ignite grid
    /// </summary>
    public static class RaptorClientServerFactory
    {
        /// <summary>
        /// Creates an appropriate new Ignite client node depending on the Raptor Grid it is being attached to
        /// </summary>
        /// <param name="gridName"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static RaptorIgniteServer NewClientNode(string gridName, string role)
        {
            if (gridName.Equals(RaptorGrids.RaptorMutableGridName()))
            {
                return new RaptorMutableClientServer(role);
            }
            else if (gridName.Equals(RaptorGrids.RaptorImmutableGridName()))
            {
                return new RaptorImmutableClientServer(role);
            }
            else
            {
                throw new ArgumentException($"{gridName} is an unknown grid to create a client node within.");
            }

        }
    }
}
