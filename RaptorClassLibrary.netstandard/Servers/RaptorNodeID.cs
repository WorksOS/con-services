using Apache.Ignite.Core;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage;

namespace VSS.TRex.Servers
{
    /// <summary>
    /// Determines and caches the TRexNodeID set in the attributes for the local Ignite node 
    /// </summary>
    public class RaptorNodeID
    {
        /// <summary>
        /// Local storage for the 
        /// </summary>
        private static string[] raptorNodeIDs = {"", ""}; 

        public static string ThisNodeID(StorageMutability mutability)
        {
            if (raptorNodeIDs[(int)mutability] == "")
            {
                raptorNodeIDs[(int)mutability] = Ignition.GetIgnite(TRexGrids.GridName(mutability)).GetCluster().GetLocalNode().GetAttribute<string>("TRexNodeID");
            }

            return raptorNodeIDs[(int)mutability];
        }
    }
}
