using Apache.Ignite.Core;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage;

namespace VSS.TRex.Servers
{
    /// <summary>
    /// Determines and caches the TRexNodeId set in the attributes for the local Ignite node 
    /// </summary>
    public class TRexNodeID
    {
        /// <summary>
        /// Local storage for the 
        /// </summary>
        private static string[] tRexNodeIDs = {"", ""}; 

        public static string ThisNodeID(StorageMutability mutability)
        {
            if (tRexNodeIDs[(int)mutability] == "")
            {
                tRexNodeIDs[(int)mutability] = Ignition.GetIgnite(TRexGrids.GridName(mutability)).GetCluster().GetLocalNode().GetAttribute<string>("TRexNodeId");
            }

            return tRexNodeIDs[(int)mutability];
        }
    }
}
