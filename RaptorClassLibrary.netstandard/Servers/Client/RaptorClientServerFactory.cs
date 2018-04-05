using System;
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
            if (gridName.Equals(RaptorGrids.RaptorImmutableGridName()))
            {
                return new RaptorImmutableClientServer(role);
            }

            throw new ArgumentException($"{gridName} is an unknown grid to create a client node within.");
        }
    }
}
