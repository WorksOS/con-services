using Apache.Ignite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// Determines and caches the RaptorNodeID set in the attributes for the local Ignite node 
    /// </summary>
    public class RaptorNodeID
    {
        /// <summary>
        /// Local storage for the 
        /// </summary>
        private static string[] raptorNodeIDs= new string[2] {"", ""}; 

        public static string ThisNodeID(StorageMutability mutability)
        {
            if (raptorNodeIDs[(int)mutability] == "")
            {
                raptorNodeIDs[(int)mutability] = Ignition.GetIgnite(RaptorGrids.RaptorGridName(mutability)).GetCluster().GetLocalNode().GetAttribute<string>("RaptorNodeID");
            }

            return raptorNodeIDs[(int)mutability];
        }
    }
}
