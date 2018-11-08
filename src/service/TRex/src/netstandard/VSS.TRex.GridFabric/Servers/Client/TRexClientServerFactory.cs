using System;
using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.Servers.Client
{
    /// <summary>
    /// A factory class responsible for creating mutable or immutable TRex client nodes in the Ignite grid
    /// </summary>
    public static class TRexClientServerFactory
    {
        /// <summary>
        /// Creates an appropriate new Ignite client node depending on the TRex Grid it is being attached to
        /// </summary>
        /// <param name="gridName"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static IgniteServer NewClientNode(string gridName, string role)
        {
            if (gridName.Equals(TRexGrids.MutableGridName()))
            {
                return new MutableClientServer(role);
            }
            if (gridName.Equals(TRexGrids.ImmutableGridName()))
            {
                return new ImmutableClientServer(role);
            }

            throw new ArgumentException($"{gridName} is an unknown grid to create a client node within.");
        }
    }
}
