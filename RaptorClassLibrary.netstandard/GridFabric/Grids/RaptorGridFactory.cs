using Apache.Ignite.Core;
using System;

namespace VSS.TRex.GridFabric.Grids
{
    public static class RaptorGridFactory
    {
        /// <summary>
        /// Creates an appropriate new Ignite grid reference depending on the Raptor Grid passed in
        /// </summary>
        /// <param name="gridName"></param>
        /// <returns></returns>
        public static IIgnite Grid(string gridName)
        {
            if (gridName.Equals(TRexGrids.MutableGridName()))
            {
                return Ignition.TryGetIgnite(gridName);
            }
            if (gridName.Equals(TRexGrids.ImmutableGridName()))
            {
                return Ignition.TryGetIgnite(gridName);
            }

            throw new ArgumentException($"{gridName} is an unknown grid to create a reference for.");
        }
    }
}
